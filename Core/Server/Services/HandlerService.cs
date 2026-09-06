using System.Reflection;
using System.Xml;
using Server.Attributes;
using System.Xml.Linq;
using System.Xml.Serialization;
using Abstractions.Handlers;
using Abstractions.Services;
using Server.Plugins;

namespace Server.Services;

public class HandlerService(IServiceScopeFactory serviceScopeFactory, ILogger<HandlerService> logger, IXmlLogService xmlLogService, PluginRegistry pluginRegistry) : IHandlerService
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly ILogger<HandlerService> _logger = logger;

    public List<Type> Handlers { get; set; } = [];

    public async Task<XDocument> Handle(string model, string module, string method, XDocument body)
    {
        var gameModel = GameModel.Parse(model);

        // ── Built-in server handlers (host SP) ──────────────────────────────
        foreach(var handler in Handlers)
        {
            //Module and service are on the attribute
            var handlerAttribute = handler.CustomAttributes.FirstOrDefault(x => x.AttributeType == typeof(HandlerAttribute));

            if(handlerAttribute is null)
            {
                await using var hostScope = _serviceScopeFactory.CreateAsyncScope();
                var response = await HandleInheritanceClass(handler, gameModel, module, method, body, hostScope.ServiceProvider);

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

            var document = new XDocument();//await handlerInstance.HandleAsync(model);

            return document;
        }

        // ── Plugin handlers (composite SP: plugin first → host fallback) ────
        foreach (var slot in pluginRegistry.GetSlots())
        {
            await using var hostScope = _serviceScopeFactory.CreateAsyncScope();
            await using var pluginScope = slot.PluginServices.CreateAsyncScope();
            var composite = new PluginServiceProvider(pluginScope.ServiceProvider, hostScope.ServiceProvider);

            foreach (var handlerType in slot.HandlerTypes)
            {
                var result = await HandleInheritanceClass(handlerType, gameModel, module, method, body, composite);
                if (result is not null) return result;
            }
        }

        //If no handler is found return an empty document
        _logger.LogWarning("No handler found for {model}/{module}/{method}", model, module, method);
        _logger.LogWarning("Document was {document}", body);

        return new XDocument();
    }

    private async Task<XDocument?> HandleInheritanceClass(Type handler, GameModel model, string module, string method, XDocument body, IServiceProvider serviceProvider)
    {
        var isHandlerWithoutRequest = false;

        var requiredXdocumentConstructor = handler.GetConstructors()
            .Any(c => c.GetParameters().Any(p => p.ParameterType == typeof(XDocument)));

        if (handler.IsAbstract)
        {
            throw new InvalidOperationException($"Cannot create instance of abstract class: {handler.FullName}");
        }

        if(ActivatorUtilities.CreateInstance(serviceProvider, handler) is not BaseHandler handlerInstance)
        {
            return null;
        }

        var baseType = handler.BaseType;

        if (baseType is { IsGenericType: true })
        {
            var genericDef = baseType.GetGenericTypeDefinition();

            if (genericDef == typeof(HandlerWithoutRequest<>))
            {
                isHandlerWithoutRequest = true;
                handlerInstance.Definition =
                    new HandlerDefinition(handler, baseType.GenericTypeArguments[0], typeof(object));
            }
            else
            {
                handlerInstance.Definition =
                    new HandlerDefinition(handler, baseType.GenericTypeArguments[0], baseType.GenericTypeArguments[1]);
            }
        }
        
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

        var handlerModule = handlerInstance.Definition.HandlerModule;
        var handlerMethod = handlerInstance.Definition.HandlerMethod;
        var handlerGameCode = handlerInstance.Definition.HandlerGameCode;
        var handlerMinVer = handlerInstance.Definition.HandlerMinVer;
        var handlerMaxVer = handlerInstance.Definition.HandlerMaxVer;

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
            if(handlerGameCode != model.GameCode)
                return null;
        }

        if(!string.IsNullOrEmpty(handlerMinVer))
        {
            var parsedMinVer = long.Parse(handlerMinVer);

            if(model.Version < parsedMinVer)
                return null;
        }

        if(!string.IsNullOrEmpty(handlerMaxVer))
        {
            var parsedMaxVer = long.Parse(handlerMaxVer);
            if(model.Version > parsedMaxVer)
                return null;
        }

        if(handlerModule != module || handlerMethod != method)
            return null;
        
        if (handler.IsAbstract)
        {
            throw new InvalidOperationException($"Cannot create instance of abstract class: {handler.FullName}");
        }

        object? request = null;
        
        if (!isHandlerWithoutRequest)
        {
            var modelType = handlerInstance.Definition.RequestType;

            var serializer = new XmlSerializer(modelType);
            
            var callElement = body.Element("call");

            if (callElement is null)
            {
                _logger.LogError("Call element not found in request");
                return null;           
            }
            
            request = serializer.Deserialize(callElement.FirstNode!.CreateReader());
        }
        
        XElement? responseElement = null;
        
        MethodInfo? handleMethod;
        MethodInfo? handleAsyncMethod;

        var methods = handler.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
        if (isHandlerWithoutRequest)
        {
            handleMethod = methods.FirstOrDefault(m => m.Name == "Handle" && m.GetParameters().Length == 1);
            handleAsyncMethod = methods.FirstOrDefault(m => m.Name == "HandleAsync" && m.GetParameters().Length == 1);
        }
        else
        {
            handleMethod = methods.FirstOrDefault(m =>
                m.Name == "Handle" &&
                m.GetParameters().Length == 2
            );
            handleAsyncMethod = methods.FirstOrDefault(m =>
                m.Name == "HandleAsync" &&
                m.GetParameters().Length == 2
            );
        }

        var parameters = new List<object>();
        if (!isHandlerWithoutRequest && request is not null)
        {
            parameters.Add(request);
        }
        parameters.Add(model);

        if (handleMethod != null)
        {
            // Synchronous Handle
            var result = handleMethod.Invoke(handlerInstance, parameters.ToArray());
            if (result is not null)
                responseElement = ToXElement(result);
        }
        else if (handleAsyncMethod != null)
        {
            dynamic task = handleAsyncMethod.Invoke(handlerInstance, parameters.ToArray())!;
            await task;
            var result = task.GetAwaiter().GetResult();
            responseElement = ToXElement(result);
        }

        if (responseElement != null)
        {
            var firstElementName = responseElement.Name;
            if (firstElementName == "response")
            {
                return responseElement.Document;
            }
            var responseDocument = new XDocument(new XElement("response", responseElement));
            return responseDocument;
        }

        _logger.LogError("Handle and HandleAsync not implemented for {Handler}", handler.Name);
        return null;
    }

    private static XElement ToXElement(object obj)
    {
        if (obj is XDocument xDocument)
        {
            return xDocument.Root!;
        }
        
        var serializer = new XmlSerializer(obj.GetType());

        var ns = new XmlSerializerNamespaces();
        ns.Add(string.Empty, string.Empty);

        using var writer = new StringWriter();
        serializer.Serialize(writer, obj, ns);

        return XElement.Parse(writer.ToString());
    }
}
