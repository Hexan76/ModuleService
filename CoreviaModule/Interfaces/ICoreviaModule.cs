using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Corevia.Dependency.Module
{
    public interface ICoreviaModule
    {
        void PreConfigureServices(IServiceCollection services, IConfiguration configuration) { }
        void ConfigureServices(IServiceCollection services, IConfiguration configuration) { }
        void PostConfigureServices(IServiceCollection services, IConfiguration configuration) { }
    }
}