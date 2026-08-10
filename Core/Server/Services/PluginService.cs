using System.Diagnostics;
using System.Reflection;
using Abstractions;
using Abstractions.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Server.Plugins;
using Path = System.IO.Path;

namespace Server.Services;

public class PluginService(ILogger logger, PluginRegistry pluginRegistry) : IPluginService
{
    private readonly string _pluginPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins");
    private readonly List<PluginSlot> _pendingSlots = [];
    private IServiceProvider? _serviceProvider;
    private WebApplication? _webApplication;

    public void SetServiceProvider(IServiceProvider serviceProvider) =>
        _serviceProvider = serviceProvider;

    // ── Phase 1: before builder.Build() ─────────────────────────────────────

    public async Task DiscoverPluginsAsync(WebApplicationBuilder builder)
    {
        var watch = Stopwatch.StartNew();

        logger.LogInformation("Discovering plugins in {path}", _pluginPath);

        if (!Directory.Exists(_pluginPath))
            Directory.CreateDirectory(_pluginPath);

        var dirs = Directory.EnumerateDirectories(_pluginPath).ToArray();

        if (dirs.Length == 0)
        {
            logger.LogInformation("No plugin directories found");
            return;
        }

        var loaded = 0;
        for (var i = 0; i < dirs.Length; i++)
        {
            var dll = FindPluginDll(dirs[i]);
            if (dll is null)
            {
                logger.LogWarning("Skipping directory '{dir}': no DLL found", Path.GetFileName(dirs[i]));
                continue;
            }

            var slot = TryLoadSlot(dll);
            if (slot is null) continue; // TryLoadSlot already logged the reason

            logger.LogInformation("Registering plugin {} (gameCode={gameCode}, {verRange})",
                slot.Plugin.Name, slot.Plugin.GameCode, VerRange(slot.Plugin.MinVer, slot.Plugin.MaxVer));
            await slot.Plugin.OnBuilderInitialize(builder);
            _pendingSlots.Add(slot);
            loaded++;
            logger.LogInformation("Registered plugin {currentCount} of {fullCount}", loaded, dirs.Length);
        }

        watch.Stop();
        logger.LogInformation("Registered {registeredPluginsCount} plugin(s) in {elapsed} ms",
            loaded, watch.ElapsedMilliseconds);
    }

    // ── Phase 2: after builder.Build() ──────────────────────────────────────

    public async Task ActivatePluginsAsync(WebApplication app)
    {
        _webApplication = app;

        foreach (var slot in _pendingSlots)
        {
            pluginRegistry.Add(slot);
            await slot.Plugin.OnAppInitialize(app, slot.PluginServices);
            logger.LogInformation("Plugin '{name}' activated (gameCode={gameCode}, {verRange})",
                slot.Plugin.Name, slot.Plugin.GameCode, VerRange(slot.Plugin.MinVer, slot.Plugin.MaxVer));
        }

        _pendingSlots.Clear();
    }

    // ── Hot-reload ───────────────────────────────────────────────────────────

    public async Task ReloadAsync(string pluginDllPath)
    {
        var name = Path.GetFileNameWithoutExtension(pluginDllPath);
        logger.LogInformation("Hot-reloading plugin from '{dll}'", name);

        var newSlot = TryLoadSlot(pluginDllPath);
        if (newSlot is null) return; // TryLoadSlot already logged the reason

        var old = pluginRegistry.Remove(newSlot.Key);

        if (old is not null)
            logger.LogInformation("Plugin '{name}' (gameCode={gameCode}, {verRange}) unloaded",
                old.Plugin.Name, old.Plugin.GameCode, VerRange(old.Plugin.MinVer, old.Plugin.MaxVer));

        pluginRegistry.Add(newSlot);

        if (_webApplication is not null)
            await newSlot.Plugin.OnAppInitialize(_webApplication, newSlot.PluginServices);

        old?.Unload();
        logger.LogInformation("Plugin '{name}' v{version} hot-reloaded successfully (gameCode={gameCode}, {verRange})",
            newSlot.Plugin.Name, newSlot.Plugin.Version,
            newSlot.Plugin.GameCode, VerRange(newSlot.Plugin.MinVer, newSlot.Plugin.MaxVer));
    }

    public Task UnloadAsync(string deletedPath)
    {
        // deletedPath can be the plugin's DLL itself (deleted in place) or the plugin's
        // whole subdirectory removed wholesale (e.g. `rm -rf plugins/Foo`) - match either
        // the exact assembly path or anything loaded from underneath the deleted directory.
        var prefix = deletedPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;

        var slots = pluginRegistry.GetSlots()
            .Where(s => s.Context.AssemblyPath == deletedPath
                        || s.Context.AssemblyPath.StartsWith(prefix, StringComparison.Ordinal))
            .ToList();

        if (slots.Count == 0)
        {
            logger.LogWarning("No loaded plugin matches deleted path '{path}'", deletedPath);
            return Task.CompletedTask;
        }

        foreach (var slot in slots)
        {
            pluginRegistry.Remove(slot.Key);
            slot.Unload();

            logger.LogInformation("Plugin '{name}' (gameCode={gameCode}, {verRange}) unloaded because '{path}' was deleted",
                slot.Plugin.Name, slot.Plugin.GameCode, VerRange(slot.Plugin.MinVer, slot.Plugin.MaxVer), deletedPath);
        }

        return Task.CompletedTask;
    }

    // ── Queries ──────────────────────────────────────────────────────────────

    public IEnumerable<IMedusaPlugin> GetPlugins() =>
        pluginRegistry.GetPlugins();

    public IMedusaPlugin? FindPlugin(string gameCode, int? minVer = null, int? maxVer = null) =>
        pluginRegistry.GetPlugins()
            .Where(p => p.GameCode == gameCode)
            .Where(p => minVer is null || p.MinVer <= minVer)
            .Where(p => maxVer is null || p.MaxVer >= maxVer)
            .FirstOrDefault();

    public async Task<bool> DoesProfileExistAsync(IMedusaPlugin plugin, string cardId)
    {
        if (_serviceProvider is null)
            throw new InvalidOperationException("PluginService has not been initialized with a service provider yet.");

        var slot = pluginRegistry.GetSlots()
            .FirstOrDefault(s => s.Plugin.GameCode == plugin.GameCode
                                 && s.Plugin.MinVer == plugin.MinVer
                                 && s.Plugin.MaxVer == plugin.MaxVer);

        var method = plugin.DoesProfileExist;
        var parameters = method.Method.GetParameters();

        await using var hostScope = _serviceProvider.CreateAsyncScope();

        IServiceProvider composite;
        if (slot is not null)
        {
            await using var pluginScope = slot.PluginServices.CreateAsyncScope();
            composite = new PluginServiceProvider(pluginScope.ServiceProvider, hostScope.ServiceProvider);
        }
        else
        {
            composite = hostScope.ServiceProvider;
        }

        var args = parameters.Select(p =>
        {
            if (p.Name == "cardId") return (object)cardId;
            var isFromServices = p.GetCustomAttribute<FromServicesAttribute>() != null;
            return isFromServices
                ? composite.GetRequiredService(p.ParameterType)
                : throw new InvalidOperationException(
                    $"Don't know how to resolve parameter '{p.Name}' on plugin delegate DoesProfileExist");
        }).ToArray();

        return await (Task<bool>)method.DynamicInvoke(args)!;
    }

    // ── Internal ─────────────────────────────────────────────────────────────

    private PluginSlot? TryLoadSlot(string dllPath)
    {
        var context = new PluginLoadContext(dllPath);

        Assembly assembly;
        try
        {
            assembly = context.LoadFromAssemblyPath(dllPath);
        }
        catch (Exception ex)
        {
            context.Unload();
            logger.LogError("Failed to load assembly '{}': {}", Path.GetFileName(dllPath), ex.Message);
            return null;
        }

        Type? pluginType;
        IReadOnlyList<Type> handlerTypes;
        try
        {
            pluginType = assembly.GetTypes()
                .FirstOrDefault(t => t.GetInterfaces().Contains(typeof(IMedusaPlugin)));

            handlerTypes = assembly.GetTypes()
                .Where(t => typeof(BaseHandler).IsAssignableFrom(t)
                            && t is { IsAbstract: false, IsInterface: false })
                .ToList();
        }
        catch (ReflectionTypeLoadException ex)
        {
            context.Unload();
            logger.LogWarning("Plugin '{}' appears to be outdated or incompatible. Please update it to the latest version of Medusa. Details: {}",
                assembly.GetName().Name,
                string.Join("; ", ex.LoaderExceptions.Select(e => e?.Message ?? "Unknown error")));
            return null;
        }

        if (pluginType is null)
        {
            context.Unload();
            logger.LogError("Could not find medusa plugin in dll {}", assembly.GetName().Name);
            return null;
        }

        logger.LogInformation("Trying to register plugin {}", assembly.GetName().Name);

        if (Activator.CreateInstance(pluginType) is not IMedusaPlugin instance)
        {
            context.Unload();
            logger.LogError("Could not create instance of plugin {}", pluginType.Name);
            return null;
        }

        var sc = new ServiceCollection();
        instance.ConfigurePluginServices(sc);
        var pluginSp = sc.BuildServiceProvider();

        return new PluginSlot(context, instance, handlerTypes, pluginSp);
    }

    // Datecodes are YYYYMMDDXX, e.g. 2025070800 → "2025-07-08 r00"
    private static string FormatVer(int ver)
    {
        var s = ver.ToString("D10");
        return $"{s[..4]}-{s[4..6]}-{s[6..8]} r{s[8..]}";
    }

    private static string VerRange(int? minVer, int? maxVer) =>
        (minVer, maxVer) switch
        {
            (null, null) => "all versions",
            (not null, null) => $"{FormatVer(minVer.Value)}+",
            (null, not null) => $"up to {FormatVer(maxVer.Value)}",
            _ => $"{FormatVer(minVer.Value)} – {FormatVer(maxVer.Value)}"
        };

    private static string? FindPluginDll(string dir) =>
        Directory.EnumerateFiles(dir, "*.dll")
            .FirstOrDefault(f => !Path.GetFileNameWithoutExtension(f)
                .Equals("Abstractions", StringComparison.OrdinalIgnoreCase));
}
