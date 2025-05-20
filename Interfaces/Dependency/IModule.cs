using Microsoft.Extensions.DependencyInjection;

namespace HashtApp.Soft.Client.Utilities;
public interface IModule
{
    void PreConfigureServices(IServiceCollection services);
    void ConfigureServices(IServiceCollection services);
    void PostConfigureServices(IServiceCollection services);
}
