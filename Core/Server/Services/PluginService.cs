using System.Diagnostics;
using System.Reflection;
using Abstractions;
using Abstractions.Handlers;
using Microsoft.AspNetCore.Mvc;
using Server.Api;
using Server.Plugins;
using Path = System.IO.Path;

namespace Server.Services;

public class PluginService(ILogger logger, PluginRegistry pluginRegistry, PluginUiEventBroadcaster uiEventBroadcaster) : IPluginService
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
        foreach (var dir in dirs)
        {
            var slot = TryLoadSlotFromDirectory(dir);
            if (slot is null) continue;

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

    public async Task ReloadAsync(string pluginDir)
    {
        logger.LogInformation("Hot-reloading plugin in '{dir}'", Path.GetFileName(pluginDir));

        // Re-scan the whole directory rather than assuming any particular dll is the plugin -
        // package-referenced plugins ship every dependency dll (AssetsTools.NET, EFCore, ...)
        // alongside their own, so which file actually declares IMedusaPlugin isn't known up front.
        var newSlot = TryLoadSlotFromDirectory(pluginDir);
        if (newSlot is null) return; // TryLoadSlotFromDirectory already logged the reason

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
        
        if (newSlot.Plugin.UiManifest is not null)
            uiEventBroadcaster.Broadcast(
                new PluginUiEvent("reloaded", PluginsApi.DerivePluginUiId(newSlot.Plugin)));
    }

    public Task UnloadAsync(string deletedPath)
    {
        // deletedPath can be the plugin's DLL itself (deleted in place) or the plugin's
        // whole subdirectory removed wholesale (e.g. `rm -rf plugins/Foo`) - match either
        // the exact assembly path or anything loaded from underneath the deleted directory.
        var prefix = deletedPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) +
                     Path.DirectorySeparatorChar;

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

            logger.LogInformation(
                "Plugin '{name}' (gameCode={gameCode}, {verRange}) unloaded because '{path}' was deleted",
                slot.Plugin.Name, slot.Plugin.GameCode, VerRange(slot.Plugin.MinVer, slot.Plugin.MaxVer), deletedPath);
            
            if (slot.Plugin.UiManifest is not null)
                uiEventBroadcaster.Broadcast(
                    new PluginUiEvent("unloaded", PluginsApi.DerivePluginUiId(slot.Plugin)));
        }

        return Task.CompletedTask;
    }

    // ── Queries ──────────────────────────────────────────────────────────────

    public IEnumerable<IMedusaPlugin> GetPlugins() =>
        pluginRegistry.GetPlugins();

    public IMedusaPlugin? FindPlugin(string gameCode, int? minVer = null, int? maxVer = null) =>
        pluginRegistry
            .GetPlugins()
            .Where(p => p.GameCode == gameCode)
            .Where(p => minVer is null || p.MinVer <= minVer)
            .FirstOrDefault(p => maxVer is null || p.MaxVer >= maxVer);

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

        if (slot is null) return await InvokeAsync(hostScope.ServiceProvider);

        // pluginScope must stay alive until the delegate has actually run, since
        // composite (and anything resolved from it) is only valid while it's open.
        await using var pluginScope = slot.PluginServices.CreateAsyncScope();
        var composite = new PluginServiceProvider(pluginScope.ServiceProvider, hostScope.ServiceProvider);
        return await InvokeAsync(composite);

        Task<bool> InvokeAsync(IServiceProvider serviceProvider)
        {
            var args = parameters.Select(p =>
            {
                if (p.Name == "cardId") return cardId;
                var isFromServices = p.GetCustomAttribute<FromServicesAttribute>() != null;
                return isFromServices
                    ? serviceProvider.GetRequiredService(p.ParameterType)
                    : throw new InvalidOperationException(
                        $"Don't know how to resolve parameter '{p.Name}' on plugin delegate DoesProfileExist");
            }).ToArray();

            return (Task<bool>)method.DynamicInvoke(args)!;
        }
    }

    // ── Internal ─────────────────────────────────────────────────────────────

    // A plugin folder now ships every package dependency alongside the plugin's own dll
    // (AssetsTools.NET.dll, EFCore, etc.), so which file actually declares IMedusaPlugin
    // isn't known up front - try each dll in the directory until one of them does.
    private PluginSlot? TryLoadSlotFromDirectory(string dir)
    {
        var dlls = Directory.EnumerateFiles(dir, "*.dll").ToArray();
        if (dlls.Length == 0)
        {
            logger.LogWarning("Skipping directory '{dir}': no DLL found", Path.GetFileName(dir));
            return null;
        }

        foreach (var dllFile in dlls)
        {
            if (dllFile.Contains("Abstractions.dll"))
                continue;

            var slot = TryLoadSlot(dllFile);
            if (slot is not null) return slot;
        }

        logger.LogWarning("No medusa plugin found among {count} dll(s) in '{dir}'", dlls.Length, Path.GetFileName(dir));
        return null;
    }

    // A non-atomic deploy (plain multi-file cp, editor autosave, etc.) can leave a dll
    // partially written when FileSystemWatcher's debounce fires. That surfaces as
    // BadImageFormatException ("Invalid token", "Invalid assembly public key", ...) from
    // reading a truncated PE - a transient condition, not a real failure, so retry through
    // it briefly before giving up. Any other exception (genuinely bad/incompatible dll)
    // fails immediately as before.
    private const int LoadRetryAttempts = 5;
    private static readonly TimeSpan LoadRetryDelay = TimeSpan.FromMilliseconds(150);

    private PluginSlot? TryLoadSlot(string dllPath)
    {
        var context = new PluginLoadContext(dllPath);

        Assembly assembly;
        try
        {
            assembly = LoadWithRetry(context, dllPath);
        }
        catch (Exception ex)
        {
            context.Unload();
            logger.LogError("Failed to load assembly '{path}': {message}", Path.GetFileName(dllPath), ex.Message);
            return null;
        }

        Type? pluginType;
        IReadOnlyList<Type> handlerTypes;
        try
        {
            var pluginTypes = assembly.GetTypes();
            pluginType = pluginTypes
                .FirstOrDefault(t => t.GetInterfaces().Contains(typeof(IMedusaPlugin)));

            if (pluginType is null)
            {
                // Not the plugin dll itself - just one of its dependencies (AssetsTools.NET,
                // EFCore, ...) picked up while scanning the whole directory. Not an error on
                // its own, so no log here; TryLoadSlotFromDirectory logs if *none* of them match.
                context.Unload();
                return null;
            }

            handlerTypes = assembly.GetTypes()
                .Where(t => typeof(BaseHandler).IsAssignableFrom(t)
                            && t is { IsAbstract: false, IsInterface: false })
                .ToList();
        }
        catch (ReflectionTypeLoadException ex)
        {
            context.Unload();
            logger.LogWarning(
                "Plugin '{name}' appears to be outdated or incompatible. Please update it to the latest version of Medusa. Details: {}",
                assembly.GetName().Name,
                string.Join("; ", ex.LoaderExceptions.Select(e => e?.Message ?? "Unknown error")));
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

    private Assembly LoadWithRetry(PluginLoadContext context, string dllPath)
    {
        var attempt = 1;

        do
        {
            try
            {
                return LoadFromPathAsStream(context, dllPath);
            }
            catch (BadImageFormatException) when (attempt < LoadRetryAttempts)
            {
                logger.LogDebug(
                    "'{path}' looks like a partial write (attempt {attempt}/{retryAttempts}), retrying...",
                    Path.GetFileName(dllPath),
                    attempt,
                    LoadRetryAttempts);

                Thread.Sleep(LoadRetryDelay);
            }

            attempt++;
        } while (attempt <= LoadRetryAttempts);

        throw new InvalidOperationException("Failed to load assembly.");
    }

    private static Assembly LoadFromPathAsStream(PluginLoadContext context, string dllPath)
    {
        using var dllStream = new MemoryStream(File.ReadAllBytes(dllPath));

        var pdbPath = Path.ChangeExtension(dllPath, ".pdb");
        if (!File.Exists(pdbPath))
            return context.LoadFromStream(dllStream);

        using var pdbStream = new MemoryStream(File.ReadAllBytes(pdbPath));
        return context.LoadFromStream(dllStream, pdbStream);
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
}