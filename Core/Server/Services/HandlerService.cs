using Server.Attributes;
using Server.Handlers;
using System.Xml.Linq;
using Abstractions.Handlers;

namespace Server.Services;

public class HandlerService(IServiceScopeFactory serviceScopeFactory, ILogger<HandlerService> logger) : IHandlerService
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly ILogger<HandlerService> _logger = logger;

    public List<Type> Handlers { get; set; } = [];

    public async Task<XDocument> Handle(string model, string module, string method, XDocument body)
    {
        foreach(var handler in Handlers)
        {
            //Module and service are on the attribute
            var handlerAttribute = handler.CustomAttributes.FirstOrDefault(x => x.AttributeType == typeof(HandlerAttribute));

            if(handlerAttribute is null)
            {
                var response = await HandleInheritanceClass(handler, model, module, method, body);

                if(response is not null)
                {
                    return response;
                }

                continue;
            }

            var attributeModule = handlerAttribute.ConstructorArguments[0].Value!.ToString();
            var attributeMethod = handlerAttribute.ConstructorArguments[1].Value!.ToString();

            if (attributeModule != module || attributeMethod != method) continue;
            await using var scope = _serviceScopeFactory.CreateAsyncScope();

            // Body param is optional, so check for it
            var requiresXDocumentConstructor = handler.GetConstructors()
                .Any(c => c.GetParameters().Any(p => p.ParameterType == typeof(XDocument)));
            var handlerInstance = requiresXDocumentConstructor
                ? (IHandler)ActivatorUtilities.CreateInstance(scope.ServiceProvider, handler, body)
                : (IHandler)ActivatorUtilities.CreateInstance(scope.ServiceProvider, handler);

            return await handlerInstance.HandleAsync(model);
        }

        //If no handler is found return an empty document
        _logger.LogWarning($"No handler found for {model}/{module}/{method}");

        return new XDocument();
    }

    private async Task<XDocument?> HandleInheritanceClass(Type handler, string model, string module, string method, XDocument body)
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();

        bool requiredXdocumentConstructor = handler.GetConstructors()
            .Any(c => c.GetParameters().Any(p => p.ParameterType == typeof(XDocument)));

        Handler handlerInstance = requiredXdocumentConstructor
            ? (Handler)ActivatorUtilities.CreateInstance(scope.ServiceProvider, handler, body)
            : (Handler)ActivatorUtilities.CreateInstance(scope.ServiceProvider, handler);

        try
        {
            var configureMethod = handler.GetMethod("Configure");
            configureMethod?.Invoke(handlerInstance, null);
        }
        catch(NotImplementedException e)
        {
            _logger.LogError(e, "Configure method not implemented for {Handler}", handler.Name);
            return null;
        }

        var modelParts = model.Split(':');

        var handlerModule = handlerInstance.HandlerModule;
        var handlerMethod = handlerInstance.HandlerMethod;
        var handlerGameCode = handlerInstance.HandlerGameCode;
        var handlerMinVer = handlerInstance.HandlerMinVer;
        var handlerMaxVer = handlerInstance.HandlerMaxVer;

        if(string.IsNullOrEmpty(handlerModule))
        {
            _logger.LogError("Module not set for {Handler}", handler.Name);
            return null;
        }

        if(string.IsNullOrEmpty(handlerMethod))
        {
            _logger.LogError("Method not set for {Handler}", handler.Name);
            return null;
        }

        if(!string.IsNullOrEmpty(handlerGameCode))
        {
            if(handlerGameCode != modelParts[0])
                return null;
        }

        var version = string.Join(string.Empty, modelParts.Skip(4));

        if(!string.IsNullOrEmpty(handlerMinVer))
        {
            var parsedVersion = int.Parse(version);
            var parsedMinVer = int.Parse(handlerMinVer);

            if(parsedVersion < parsedMinVer)
                return null;
        }

        if(!string.IsNullOrEmpty(handlerMaxVer))
        {
            var parsedVersion = int.Parse(version);
            var parsedMaxVer = int.Parse(handlerMaxVer);
            if(parsedVersion > parsedMaxVer)
                return null;
        }

        if(handlerModule != module || handlerMethod != method)
            return null;

        var handleMethod = handlerInstance.GetType().GetMethod("Handle", [typeof(string)]);
        var handleAsyncMethod = handlerInstance.GetType().GetMethod("HandleAsync", [typeof(string)]);
        
        var response = handleMethod?.DeclaringType != typeof(Handler)
            ? handlerInstance.Handle(model)
            : handleAsyncMethod?.DeclaringType != typeof(Handler)
                ? await handlerInstance.HandleAsync(model)
                : null;

        if (response is not null) return response;
        _logger.LogError("Handle and HandleAsync not implemented for {Handler}", handler.Name);
        return null;
    }

}
