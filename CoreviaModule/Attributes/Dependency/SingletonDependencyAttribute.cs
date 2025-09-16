using System;

namespace Corevia.Dependency.Module
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class)]
    public class SingletonDependencyAttribute : Attribute
    {
    }
}
