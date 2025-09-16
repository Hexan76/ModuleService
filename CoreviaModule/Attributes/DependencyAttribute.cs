using System;

namespace Corevia.Dependency.Module
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class DependencyAttribute : Attribute
    {
        public bool Replace { get; set; }

        public DependencyAttribute()
        {
            
        }
    }
}
