using System.Reflection;
using HotChocolate.Execution.Configuration;

namespace Server.Extensions;

public static class RequestExecutorBuilderExtensions
{
    extension(IRequestExecutorBuilder builder)
    {
        public IRequestExecutorBuilder AddTypeExtensionsInNamespaceOf(Type type)
        {
            var extensionNamespace = type.Namespace;

            return extensionNamespace is null ? 
                builder : builder.AddTypeExtensionsInNamespace(extensionNamespace);
        }

        public IRequestExecutorBuilder AddTypeExtensionsInNamespaceOf<T>()
        {
            var type = typeof(T);
            var extensionNamespace = type.Namespace;

            return extensionNamespace is null ? 
                builder : builder.AddTypeExtensionsInNamespace(extensionNamespace);
        }

        private IRequestExecutorBuilder AddTypeExtensionsInNamespace(string extensionNamespace)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            var foundAssembly = assemblies.FirstOrDefault(x => x.DefinedTypes.Any(typeInfo => typeInfo.Namespace?.Equals(extensionNamespace) ?? false));
            if(foundAssembly is null)
            {
                try
                {
                    var loaded = Assembly.Load(extensionNamespace.Split('.')[0]);
                    foundAssembly = loaded;
                }
                catch(Exception ex)
                {
                    return builder;
                }
            }

            var attributeType = typeof(ExtendObjectTypeAttribute);

            var foundTypes = foundAssembly.DefinedTypes
                .Where(typeInfo =>
                    (typeInfo.Namespace?.Equals(extensionNamespace) ?? false) &&
                    typeInfo.GetCustomAttributes().Any(x => x.GetType() == attributeType))
                .ToArray();
            
            foreach(var type in foundTypes)
            {
                var customAttributes = type.GetCustomAttributes();
                if (customAttributes.Any(x => x.GetType() == attributeType))
                    builder.AddType(type.AsType());
            }

            return builder;
        }
    }
}