using Server.Handlers;
using Server.Services;
using System.Reflection;
using Abstractions.Handlers;

namespace Server.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseHandlers(this IApplicationBuilder app)
    {
        var handlerService = app.ApplicationServices.GetRequiredService<IHandlerService>();

        var assembly = Assembly.GetEntryAssembly() ?? throw new InvalidOperationException("Could not find entry assembly.");
        var types = assembly.GetTypes().Where(a => a.GetInterfaces().Contains(typeof(IHandler)) ||
        (a.IsSubclassOf(typeof(Handler)) && !a.IsAbstract));

        handlerService.Handlers.AddRange(types);

        return app;
    }

}