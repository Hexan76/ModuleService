using HashtApp.Soft.Client.Utilities;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void RegisterServices(this IServiceCollection services, params Assembly[] assemblies)
    {
        // Registering the services from assemblies
        foreach (var assembly in assemblies)
        {
            RegisterModuleServices(services, assembly);
            RegisterDependencyServices(services, assembly);
        }
    }

    private static void RegisterModuleServices(IServiceCollection services, Assembly assembly)
    {
        var moduleTypes = assembly.GetTypes()
                    .Where(t => typeof(HashtModule).IsAssignableFrom(t) && !t.IsAbstract)
                    .ToList();

        var loadedModules = new HashSet<Type>();
        var moduleInstances = new List<HashtModule>();

        foreach (var moduleType in moduleTypes)
        {
            LoadModuleWithDependencies(moduleType, services, loadedModules, moduleInstances);
        }
        // Register and configure modules
        foreach (var module in moduleInstances)
        {
            // Pre-configuration step (if any)
            module.PreConfigureServices(services);

            // Regular configuration step
            module.ConfigureServices(services);

            // Post-configuration step (if any)
            module.PostConfigureServices(services);
        }
    }

    private static void RegisterDependencyServices(IServiceCollection services, Assembly assembly)
    {
        // Register Singleton services
        var singletonServices = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && (typeof(ISingletonDependency).IsAssignableFrom(t)
            || t.GetCustomAttribute<SingletonDependencyAttribute>() != null));

        // Register Scope services
        var scopeServices = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && (typeof(IScopeDependency).IsAssignableFrom(t)
            || t.GetCustomAttribute<ScopeDependencyAttribute>() != null));

        // Register Transient services
        var transientServices = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && (typeof(ITransientDependency).IsAssignableFrom(t)
            || t.GetCustomAttribute<TransientDependencyAttribute>() != null));

        // Register singleton services
        foreach (var sService in singletonServices)
        {
            var interfaces = sService.GetInterfaces().Where(c => c.Name.EndsWith(sService.Name));
            foreach (var iface in interfaces)
            {
                services.AddSingleton(iface, sService);
            }
        }

        // Register scope services
        foreach (var sService in scopeServices)
        {
            var interfaces = sService.GetInterfaces().Where(c => c.Name.EndsWith(sService.Name));
            foreach (var iface in interfaces)
            {
                services.AddScoped(iface, sService);
            }
        }

        // Register transient services
        foreach (var sService in transientServices)
        {
            var interfaces = sService.GetInterfaces().Where(c => c.Name.EndsWith(sService.Name));
            foreach (var iface in interfaces)
            {
                services.AddTransient(iface, sService);
            }
        }
    }

    private static void LoadModuleWithDependencies(
        Type moduleType,
        IServiceCollection services,
        HashSet<Type> loadedModules,
        List<HashtModule> moduleInstances)
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

        var instance = (HashtModule)Activator.CreateInstance(moduleType)!;
        moduleInstances.Add(instance);
        loadedModules.Add(moduleType);
    }
}
