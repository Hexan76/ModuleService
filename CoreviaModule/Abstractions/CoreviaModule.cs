using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace Corevia.Dependency.Module
{
    public abstract class CoreviaModule : ICoreviaModule
    {
        public virtual void PreConfigureServices(IServiceCollection services, IConfiguration configuration) { }
        public virtual void ConfigureServices(IServiceCollection services, IConfiguration configuration) { }
        public virtual void PostConfigureServices(IServiceCollection services, IConfiguration configuration) { }

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
}
