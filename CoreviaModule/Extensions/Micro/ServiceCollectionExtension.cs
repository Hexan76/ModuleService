using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Corevia.Dependency.Module;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddScopedExposedServicesFrom(this IServiceCollection services, IEnumerable<Type> types)
        {
            foreach (var implType in types.Where(t => t.IsClass && !t.IsAbstract))
            {
                var exposeAttr = implType.GetCustomAttribute<ExposeServicesAttribute>();
                if (exposeAttr != null)
                {
                    foreach (var serviceType in exposeAttr.Types)
                    {
                        services.AddScoped(serviceType, implType);
                    }
                }
                else if (typeof(IScopeDependency).IsAssignableFrom(implType))
                {
                    // fallback: register by convention (matching name) or all interfaces
                    var interfaces = implType.GetInterfaces()
                                             .Where(i => i != typeof(IScopeDependency));
                    foreach (var iface in interfaces)
                    {
                        services.AddScoped(iface, implType);
                    }
                }
            }

            return services;
        }
        public static IServiceCollection AddSingletonExposedServicesFrom(this IServiceCollection services, IEnumerable<Type> types)
        {
            foreach (var implType in types.Where(t => t.IsClass && !t.IsAbstract))
            {
                var exposeAttr = implType.GetCustomAttribute<ExposeServicesAttribute>();
                if (exposeAttr != null)
                {
                    foreach (var serviceType in exposeAttr.Types)
                    {
                        services.AddSingleton(serviceType, implType);
                    }
                }
                else if (typeof(ISingletonDependency).IsAssignableFrom(implType))
                {
                    // fallback: register by convention (matching name) or all interfaces
                    var interfaces = implType.GetInterfaces()
                                             .Where(i => i != typeof(ISingletonDependency));
                    foreach (var iface in interfaces)
                    {
                        services.AddSingleton(iface, implType);
                    }
                }
            }

            return services;
        }
        public static IServiceCollection AddTransientExposedServicesFrom(this IServiceCollection services, IEnumerable<Type> types)
        {
            foreach (var implType in types.Where(t => t.IsClass && !t.IsAbstract))
            {
                var exposeAttr = implType.GetCustomAttribute<ExposeServicesAttribute>();
                if (exposeAttr != null)
                {
                    foreach (var serviceType in exposeAttr.Types)
                    {
                        services.AddSingleton(serviceType, implType);
                    }
                }
                else if (typeof(ITransientDependency).IsAssignableFrom(implType))
                {
                    // fallback: register by convention (matching name) or all interfaces
                    var interfaces = implType.GetInterfaces()
                                             .Where(i => i != typeof(ITransientDependency));
                    foreach (var iface in interfaces)
                    {
                        services.AddSingleton(iface, implType);
                    }
                }
            }

            return services;
        }
    }
}
