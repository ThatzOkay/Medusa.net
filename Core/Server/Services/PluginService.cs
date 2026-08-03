using System.Diagnostics;
using System.Reflection;
using Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Server.Services;

public class PluginService(ILogger logger) : IPluginService
{
    private List<IMedusaPlugin> Plugins { get; } = [];
    private readonly string _pluginPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins") ;
    private IServiceProvider? _serviceProvider;

    public void SetServiceProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void AddPlugin(IMedusaPlugin plugin)
    {
        Plugins.Add(plugin);
    }

    public void RegisterPlugins()
    {
        var watch = new Stopwatch();
        watch.Start();
        
        logger.LogInformation("Registering plugins");

        if (!Directory.Exists(_pluginPath))
            Directory.CreateDirectory(_pluginPath);
        
        var dlls = Directory.EnumerateFiles(_pluginPath, "*.dll").ToArray();
        for (var i = 0; i < dlls.Length; i++)
        {
            var assembly = Assembly.LoadFrom(dlls[i]);
            Type? medusaPlugin = null;

            try
            {
                medusaPlugin = assembly.GetTypes().FirstOrDefault(t => t.GetInterfaces().Contains(typeof(IMedusaPlugin)));
            }
            catch(ReflectionTypeLoadException ex)
            {
                logger.LogWarning("Plugin '{}' appears to be outdated or incompatible. Please update it to the latest version of Medusa. Details: {}",
                    assembly.GetName().Name,
                    string.Join("; ", ex.LoaderExceptions.Select(e => e?.Message ?? "Unknown error")));
                continue;
            }

            if (medusaPlugin is null)
            {
                logger.LogError("Could not find medusa plugin in dll {}", assembly.GetName().Name);
                continue;
            }
            
            logger.LogInformation("Trying to register plugin {}", assembly.GetName().Name);

            if (Activator.CreateInstance(medusaPlugin) is not IMedusaPlugin instance)
            {
                logger.LogError("Could not create instance of plugin {}", medusaPlugin.Name);
                continue;
            }
            
            logger.LogInformation("Registering plugin {}", instance.Name);
            AddPlugin(instance);
            logger.LogInformation("Registered plugin {currentCount} of {fullCount}", i + 1, dlls.Length);
        }
        
        watch.Stop();
        logger.LogInformation("Registered {registeredPluginsCount} plugin(s) in {elapsed} ms", dlls.Length, watch.ElapsedMilliseconds);
    }
    
    public List<IMedusaPlugin> GetPlugins()
    {
        return Plugins;
    }

    public IMedusaPlugin? FindPlugin(string gameCode, int? minVer = null, int? maxVer = null)
    {
        var foundPlugins = Plugins.Where(x => x.GameCode == gameCode);
        if (minVer != null)
            foundPlugins = foundPlugins.Where(x => x.MinVer <= minVer);
        if (maxVer != null)
            foundPlugins = foundPlugins.Where(x => x.MaxVer >= maxVer);
        return foundPlugins.FirstOrDefault();
    }

    public async Task<bool> DoesProfileExistAsync(IMedusaPlugin plugin, string cardId)
    {
        if (_serviceProvider is null)
            throw new InvalidOperationException("PluginService has not been initialized with a service provider yet.");

        var method = plugin.DoesProfileExist;
        var parameters = method.Method.GetParameters();

        using var scope = _serviceProvider.CreateScope();

        var args = parameters.Select(p =>
        {
            if (p.Name == "cardId") return (object)cardId;
            var isFromServices = p.GetCustomAttribute<FromServicesAttribute>() != null;
            if (isFromServices) return scope.ServiceProvider.GetRequiredService(p.ParameterType);
            throw new InvalidOperationException($"Don't know how to resolve parameter '{p.Name}' on plugin delegate DoesProfileExist");
        }).ToArray();

        return await (Task<bool>)method.DynamicInvoke(args)!;
    }
}