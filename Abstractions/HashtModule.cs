using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HashtApp.Soft.Client.Utilities;

public abstract class HashtModule : IModule
{
    public virtual void PreConfigureServices(IServiceCollection services) { }
    public virtual void ConfigureServices(IServiceCollection services) { }
    public virtual void PostConfigureServices(IServiceCollection services) { }

    protected static void PreConfigure<TOptions>(IServiceCollection services, Action<TOptions> configureAction)
        where TOptions : class
    {
        services.PostConfigure(configureAction);
    }

    protected static void Configure<TOptions>(IServiceCollection services, Action<TOptions> configureAction)
        where TOptions : class
    {
        services.Configure(configureAction);
    }

    protected static void PostConfigure<TOptions>(IServiceCollection services, Action<TOptions> configureAction)
        where TOptions : class
    {
        services.PostConfigure(configureAction);
    }
}
