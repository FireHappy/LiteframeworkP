using System;
using VContainer;

namespace LiteFramework.Core.Utility
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AutoRegisterAttribute : Attribute
    {
        public Lifetime Lifetime { get; }

        public AutoRegisterAttribute(Lifetime lifetime = Lifetime.Scoped)
        {
            Lifetime = lifetime;
        }
    }
}

