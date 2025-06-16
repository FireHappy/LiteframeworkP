using VContainer;

namespace LiteFramework
{
    /// <summary>
    /// 提供一个全局可访问Container容器,使用该容器,尽量值访问单例,避免生命周期导致的问题
    /// </summary>
    public static class GlobalContainer
    {
        private static IObjectResolver _resolver;

        public static void SetContainer(IObjectResolver resolver)
        {
            _resolver = resolver;
        }

        public static T Resolve<T>() where T : class
        {
            return _resolver?.Resolve<T>();
        }

        public static bool IsReady => _resolver != null;
    }
}
