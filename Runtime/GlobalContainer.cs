using System.Diagnostics;
using VContainer;

namespace LiteFramework
{
    /// <summary>
    /// 提供一个全局可访问Container容器,使用该容器,尽量值访问单例,避免生命周期导致的问题
    /// </summary>
    public static class GlobalContainer
    {
        private static IObjectResolver container;

        public static void SetContainer(IObjectResolver resolver)
        {
            UnityEngine.Debug.Log($"GlobalContainer setContainer resolver:{resolver}");
            GlobalContainer.container = resolver;
        }

        public static T Resolve<T>() where T : class
        {
            if (isReady)
            {
                return GlobalContainer.container?.Resolve<T>();
            }
            UnityEngine.Debug.Log("GlobalContainer container is null please set container");
            return default;
        }

        public static bool isReady => GlobalContainer.container != null;
    }
}
