using System.Reflection;
using VContainer;

namespace LiteFramework.Core.Utility
{
    public static class VContainerAutoRegister
    {
        public static void RegisterWithAttribute(IContainerBuilder builder, Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    var attr = type.GetCustomAttribute<AutoRegisterAttribute>();
                    if (attr != null && !type.IsAbstract)
                    {
                        builder.Register(type, attr.Lifetime);
                    }
                }
            }
        }
    }
}
