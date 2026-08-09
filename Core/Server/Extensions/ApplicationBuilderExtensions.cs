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
        var entryAssembly = Assembly.GetEntryAssembly() ?? throw new InvalidOperationException("Could not find entry assembly.");

        // Only register built-in server handlers here.
        // Plugin handler types live in PluginSlot.HandlerTypes and are dispatched
        // via PluginRegistry in HandlerService using the plugin's own mini-SP.
        var builtInHandlers = entryAssembly.GetTypes()
            .Where(t => t.GetInterfaces().Contains(typeof(IHandler)) ||
                        (t.IsSubclassOf(typeof(Handler<,>)) && !t.IsAbstract));

        handlerService.Handlers.AddRange(builtInHandlers);

        return app;
    }

}