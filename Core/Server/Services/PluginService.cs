using System.Diagnostics;
using System.Reflection;
using Abstractions;

namespace Server.Services;

public class PluginService(ILogger logger) : IPluginService
{
    private List<IMedusaPlugin> Plugins { get; } = [];
    private readonly string _pluginPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins") ;

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
            var assembly = Assembly.LoadFile(dlls[i]);
            var medusaPlugin = assembly.GetTypes().FirstOrDefault(t => t.GetInterfaces().Contains(typeof(IMedusaPlugin)));

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
            logger.LogInformation("Registered plugin {} of {}", i + 1, dlls.Length);
        }
        
        watch.Stop();
        logger.LogInformation("Registered {} plugin(s) in {} ms", dlls.Length, watch.ElapsedMilliseconds);
    }
    
    public List<IMedusaPlugin> GetPlugins()
    {
        return Plugins;
    }
}