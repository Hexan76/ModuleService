using System;
using Microsoft.Extensions.DependencyInjection;

namespace Corevia.Dependency.Module
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ExposeServicesAttribute : Attribute
    {
        public Type[] Types { get; }
        public ServiceLifetime Lifetime { get; }
        public bool IncludeSelf { get; set; }

        public ExposeServicesAttribute(params Type[] types)
        {
            Types = types;
        }
    }
}
