using Server.Handlers;
using Server.Services;
using System.Reflection;
using Abstractions.Handlers;
using Abstractions.Services;

namespace Server.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseHandlers(this IApplicationBuilder app)
    {
        var handlerService = app.ApplicationServices.GetRequiredService<IHandlerService>();
        var pluginService = app.ApplicationServices.GetService<IPluginService>();
        var pluginAssemblies = pluginService?.GetPlugins().Select(x => x.GetType().Assembly);
        var entryAssembly = Assembly.GetEntryAssembly() ?? throw new InvalidOperationException("Could not find entry assembly.");
        
        var assemblies = new List<Assembly> { entryAssembly };
        
        if (pluginAssemblies != null)
        {
            assemblies.AddRange(pluginAssemblies);
        }

        foreach (var types in assemblies.Select(assembly => assembly.GetTypes().Where(a => a.GetInterfaces().Contains(typeof(IHandler)) ||
                     (a.IsSubclassOf(typeof(Handler)) && !a.IsAbstract))))
        {
            handlerService.Handlers.AddRange(types);
        }

        return app;
    }

}