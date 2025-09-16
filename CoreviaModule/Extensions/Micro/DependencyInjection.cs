using Corevia.Dependency.Module;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjection
    {
        public static void RegisterServices(this IServiceCollection services, IConfiguration configuration, params Assembly[] assemblies)
        {
            // Registering the services from assemblies
            foreach (var assembly in assemblies)
            {
                RegisterModuleServices(services, configuration, assembly);
                RegisterDependencyServices(services, assembly);
            }
        }

        private static void RegisterModuleServices(IServiceCollection services, IConfiguration configuration, Assembly assembly)
        {
            var moduleTypes = assembly.GetTypes()
                        .Where(t => typeof(CoreviaModule).IsAssignableFrom(t) && !t.IsAbstract)
                        .ToList();

            var loadedCoreviaModules = new HashSet<Type>();
            var moduleInstances = new List<CoreviaModule>();

            foreach (var moduleType in moduleTypes)
            {
                LoadModuleWithDependencies(moduleType, services, loadedCoreviaModules, moduleInstances);
            }

            // Register and configure modules
            foreach (var module in moduleInstances)
            {
                //Automatic Search before customize configuration Services
                RegisterDependencyServices(services, module.GetType().Assembly);
                // Pre-configuration step (if any)
                module.PreConfigureServices(services, configuration);

                // Regular configuration step
                module.ConfigureServices(services, configuration);

                // Post-configuration step (if any)
                module.PostConfigureServices(services, configuration);
            }
        }

        private static void RegisterDependencyServices(IServiceCollection services, Assembly assembly)
        {
            var allTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract);

            foreach (var implType in allTypes)
            {
                var exposeAttr = implType.GetCustomAttribute<ExposeServicesAttribute>();
                var hasMarkerInterface = typeof(ISingletonDependency).IsAssignableFrom(implType) ||
                                         typeof(IScopeDependency).IsAssignableFrom(implType) ||
                                         typeof(ITransientDependency).IsAssignableFrom(implType);

                // CASE 1: No attribute, only marker interface => Register using interfaces
                if (exposeAttr == null && hasMarkerInterface)
                {
                    var lifetime = GetLifetimeFromMarkerInterface(implType);
                    if (lifetime == null) continue;

                    foreach (var serviceType in GetValidInterfaces(implType))
                    {
                        Register(services, serviceType, implType, lifetime.Value);
                    }

                    continue;
                }

                // CASE 2: Attribute exists
                if (exposeAttr != null)
                {
                    var lifetime = exposeAttr?.Lifetime;

                    if (lifetime == null)
                    {
                        lifetime = GetLifetimeFromMarkerInterface(implType);
                    }

                    if (lifetime == null)
                    {
                        throw new InvalidOperationException($"Cannot determine lifetime for {implType.FullName}");
                    }
                    var registered = new HashSet<Type>();

                    // Register all attribute-exposed types
                    foreach (var serviceType in exposeAttr.Types)
                    {
                        Register(services, serviceType, implType, lifetime.Value);
                        registered.Add(serviceType);
                    }

                    if (exposeAttr.IncludeSelf && !registered.Contains(implType))
                    {
                        Register(services, implType, implType, lifetime.Value);
                    }

                    if (exposeAttr.IncludeSelf && hasMarkerInterface)
                    {
                        // Also register by interfaces (but avoid duplicates)
                        foreach (var serviceType in GetValidInterfaces(implType).Where(x => !registered.Contains(x)))
                        {
                            Register(services, serviceType, implType, lifetime.Value);
                        }
                    }

                    // If IncludeSelf == false => skip marker interfaces
                    continue;
                }

                // No attribute, no marker = skip
            }
        }

        private static ServiceLifetime? GetLifetimeFromMarkerInterface(Type type)
        {
            if (typeof(ISingletonDependency).IsAssignableFrom(type)) return ServiceLifetime.Singleton;
            if (typeof(IScopeDependency).IsAssignableFrom(type)) return ServiceLifetime.Scoped;
            if (typeof(ITransientDependency).IsAssignableFrom(type)) return ServiceLifetime.Transient;
            return null;
        }

        private static IEnumerable<Type> GetValidInterfaces(Type implType)
        {
            var excluded = new[]
            {
                typeof(ISingletonDependency),
                typeof(IScopeDependency),
                typeof(ITransientDependency)
            };

            return implType.GetInterfaces().Where(i => !excluded.Contains(i));
        }

        private static void Register(IServiceCollection services, Type serviceType, Type implementationType, ServiceLifetime lifetime)
        {
            var replaceAttr = implementationType.GetCustomAttribute<DependencyAttribute>();
            var descriptor = new ServiceDescriptor(serviceType, implementationType, lifetime);

            if (replaceAttr?.Replace == true)
            {
                services.Replace(descriptor);
            }
            else
            {
                services.Add(descriptor);
            }
        }
        private static void LoadModuleWithDependencies(
                    Type moduleType,
                    IServiceCollection services,
                    HashSet<Type> loadedModules,
                    List<CoreviaModule> moduleInstances)
        {
            if (loadedModules.Contains(moduleType))
                return;

            var dependsAttr = moduleType.GetCustomAttribute<DependsOnAttribute>();
            if (dependsAttr != null)
            {
                foreach (var dependency in dependsAttr.Dependencies)
                {
                    LoadModuleWithDependencies(dependency, services, loadedModules, moduleInstances);
                }
            }

            var instance = (CoreviaModule)Activator.CreateInstance(moduleType)!;
            if (moduleInstances?.Any(t => t.GetType().Name == instance.GetType().Name) == false)
                moduleInstances.Add(instance);

            loadedModules.Add(moduleType);
        }
    }
}
