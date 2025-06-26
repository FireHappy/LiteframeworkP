using System;
using System.Collections.Generic;
using System.Reflection;
using VContainer;

namespace LiteFramework.Core.Utility
{
    public static partial class VContainerAutoRegister
    {
        public static Dictionary<Type, Lifetime> registerDict = new Dictionary<Type, Lifetime>();

        public static void Register(IContainerBuilder builder)
        {
#if UNITY_EDITOR
            Assembly assembly = Assembly.Load("Assembly-CSharp");
            foreach (var type in assembly.GetTypes())
            {
                var attr = type.GetCustomAttribute<AutoRegisterAttribute>();
                if (attr != null && !type.IsAbstract)
                {
                    builder.Register(type, attr.Lifetime);
                }
            }
#else
            UnityEngine.Debug.Log("[VContainerAutoRegister] Use Generate Code Register");
            foreach (KeyValuePair<Type, Lifetime> kv in registerDict)
            {
                UnityEngine.Debug.Log($"VContainerAutoRegister Register key:{kv.Key}, value:{kv.Value}");
                builder.Register(kv.Key, kv.Value);
            }
            registerDict.Clear();
#endif
        }
    }
}
