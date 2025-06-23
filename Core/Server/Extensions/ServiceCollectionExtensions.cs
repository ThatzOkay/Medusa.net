using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Server.Services;
using System.Collections.Generic;
using System.Reflection;
using Abstractions.Services;

namespace Server.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add handler service to DI
        /// </summary>
        /// <param name="services">The extended IServiceCollection</param>
        /// <returns></returns>
        public static IServiceCollection AddHandlers(this IServiceCollection services)
        {
            services.Add(new ServiceDescriptor(typeof(IHandlerService), typeof(HandlerService), ServiceLifetime.Singleton));
            return services;
        }

        /// <summary>
        /// Adds services implementing an interface to the DI container.
        /// </summary>
        /// <typeparam name="TType">The interface type</typeparam>
        /// <param name="serviceCollection">The extended IServiceCollection</param>
        /// <param name="lifetime">The lifetime of the services</param>
        /// <returns></returns>
        public static IServiceCollection AddIServicesWithType<TType>(this IServiceCollection serviceCollection,
            ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            var assembly = typeof(TType).Assembly;
            var types = assembly.DefinedTypes
                .Where(x => typeof(TType).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                .ToArray();

            if(types.Length == 0) return serviceCollection;

            foreach(var type in types)
            {
                serviceCollection.TryAdd(new ServiceDescriptor(typeof(TType), type.AsType(), lifetime));
            }

            return serviceCollection;
        }

        /// <summary>
        /// Adds services from a namespace that implement an interface to the DI container.
        /// </summary>
        /// <param name="serviceCollection">The extended IServiceCollection</param>
        /// <param name="interfaceNamespace">The namespace of the interfaces</param>
        /// <param name="lifetime">The lifetime of the services</param>
        /// <returns></returns>
        public static IServiceCollection AddServicesInNamespaceWithInterface(this IServiceCollection serviceCollection,
            string interfaceNamespace, ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var foundAssembly = assemblies.FirstOrDefault(x => x.DefinedTypes.Any(typeInfo => typeInfo.Namespace?.Equals(interfaceNamespace) ?? false));

            if(foundAssembly == null)
            {
                try
                {
                    var loaded = Assembly.Load(interfaceNamespace.Split('.')[0]);
                    foundAssembly = loaded;
                }
                catch
                {
                    return serviceCollection;
                }
            }

            var types = foundAssembly.DefinedTypes
                .Where(x => x.Namespace == interfaceNamespace && !x.IsInterface && !x.IsAbstract && !x.ContainsGenericParameters && !x.IsNested)
                .ToList();

            if(types.Count == 0) return serviceCollection;

            foreach(var type in types)
            {
                var interfaceType = type.GetInterfaces().FirstOrDefault();
                if(interfaceType != null)
                {
                    serviceCollection.TryAdd(new ServiceDescriptor(interfaceType, type.AsType(), lifetime));
                }
            }

            return serviceCollection;
        }

        /// <summary>
        /// Adds services from multiple namespaces to the DI container.
        /// </summary>
        /// <param name="serviceCollection">The extended IServiceCollection</param>
        /// <param name="namespaces">List of namespaces</param>
        /// <param name="lifetime">The lifetime of the services</param>
        /// <returns></returns>
        public static IServiceCollection AddServicesInNamespaces(this IServiceCollection serviceCollection,
            string[] namespaces, ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var foundAssemblies = assemblies
                .Where(a => a.DefinedTypes.Any(t => namespaces.Contains(t.Namespace)))
                .ToList();

            var notFoundNamespaces = namespaces.Except(foundAssemblies.SelectMany(a => a.GetTypes().Select(t => t.Namespace))).ToList();
            var types = new List<TypeInfo>();

            foreach(var assembly in foundAssemblies)
            {
                var foundTypes = assembly.DefinedTypes
                    .Where(x => namespaces.Contains(x.Namespace) && !x.IsInterface && !x.IsAbstract && !x.IsNested)
                    .ToArray();
                types.AddRange(foundTypes);
            }

            foreach(var notFound in notFoundNamespaces)
            {
                try
                {
                    var assemblyName = notFound?.Split('.')[0];
                    var assembly = Assembly.Load(assemblyName ?? "");
                    var foundTypes = assembly.DefinedTypes
                        .Where(x => x.Namespace == notFound && !x.IsInterface && !x.IsAbstract && !x.IsNested)
                        .ToArray();
                    types.AddRange(foundTypes);
                }
                catch { }
            }

            if(types.Count == 0) return serviceCollection;

            foreach(var type in types)
            {
                serviceCollection.TryAdd(new ServiceDescriptor(type.AsType(), type.AsType(), lifetime));
            }

            return serviceCollection;
        }

        /// <summary>
        /// Adds services from a single namespace to the DI container.
        /// </summary>
        /// <param name="serviceCollection">The extended IServiceCollection</param>
        /// <param name="namespace">The namespace to scan</param>
        /// <param name="lifetime">The lifetime of the services</param>
        /// <returns></returns>
        public static IServiceCollection AddServicesInNamespace(this IServiceCollection serviceCollection,
            string @namespace, ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var assembly = assemblies.FirstOrDefault(x => x.DefinedTypes.Any(typeInfo => typeInfo.Namespace == @namespace));

            var types = assembly?.DefinedTypes
                .Where(x => x.Namespace == @namespace && !x.IsInterface && !x.IsAbstract && !x.IsNested)
                .ToArray();

            if(types == null || types.Length == 0) return serviceCollection;

            foreach(var type in types)
            {
                serviceCollection.TryAdd(new ServiceDescriptor(type.AsType(), type.AsType(), lifetime));
            }

            return serviceCollection;
        }

        /// <summary>
        /// Adds a single class as a service to the DI container.
        /// </summary>
        /// <param name="serviceCollection">The extended IServiceCollection</param>
        /// <param name="serviceClassType">The class type to add</param>
        /// <param name="lifetime">The lifetime of the service</param>
        /// <returns></returns>
        public static IServiceCollection CreateServiceFromClass(this IServiceCollection serviceCollection,
            Type serviceClassType, ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            serviceCollection.TryAdd(new ServiceDescriptor(serviceClassType, serviceClassType, lifetime));
            return serviceCollection;
        }

        /// <summary>
        /// Adds multiple classes as services to the DI container.
        /// </summary>
        /// <param name="serviceCollection">The extended IServiceCollection</param>
        /// <param name="serviceClassTypes">A list of class types</param>
        /// <param name="lifetime">The lifetime of the services</param>
        /// <returns></returns>
        public static IServiceCollection CreateServiceFromClasses(this IServiceCollection serviceCollection,
            IEnumerable<Type> serviceClassTypes, ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            foreach(var serviceClassType in serviceClassTypes)
            {
                serviceCollection.TryAdd(new ServiceDescriptor(serviceClassType, serviceClassType, lifetime));
            }
            return serviceCollection;
        }

        /// <summary>
        /// Adds services based on a type that implements an interface, without adding the interface itself.
        /// </summary>
        /// <typeparam name="TType">The extended interface</typeparam>
        /// <param name="serviceDescriptors">The extended IServiceCollection</param>
        /// <param name="lifetime">The lifetime of the services</param>
        /// <returns></returns>
        public static IServiceCollection AddServicesWithType<TType>(this IServiceCollection serviceDescriptors,
            ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            var assembly = typeof(TType).Assembly;
            var types = assembly.DefinedTypes
                .Where(x => typeof(TType).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                .ToArray();

            if(types.Length == 0) return serviceDescriptors;

            foreach(var type in types)
            {
                serviceDescriptors.TryAdd(new ServiceDescriptor(type.AsType(), type.AsType(), lifetime));
            }

            return serviceDescriptors;
        }
    }

}
