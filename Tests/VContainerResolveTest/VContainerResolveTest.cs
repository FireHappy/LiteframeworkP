using UnityEngine;
using VContainer;
using VContainer.Unity;
using System.Diagnostics;


namespace LiteFramework.Tests
{
    public class VContainerResolveTest : MonoBehaviour
    {
        private IObjectResolver container;

        void Start()
        {
            var builder = new ContainerBuilder();

            // 注册 10000 个有依赖关系的类
            for (int i = 0; i < 10000; i++)
            {
                builder.Register<Dependency>(Lifetime.Scoped).AsSelf();
                builder.Register<ComponentWithDependency>(Lifetime.Scoped).AsSelf();
            }

            container = builder.Build();

            BenchmarkResolve();
        }

        void BenchmarkResolve()
        {
            const int resolveCount = 10000;
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            for (int i = 0; i < resolveCount; i++)
            {
                var instance = container.Resolve<ComponentWithDependency>();
            }

            stopwatch.Stop();

            UnityEngine.Debug.Log($"[VContainer] Resolve {resolveCount} 次耗时: {stopwatch.ElapsedMilliseconds} ms");
            UnityEngine.Debug.Log($"[VContainer] 每次解析平均耗时: {stopwatch.Elapsed.TotalMilliseconds / resolveCount:0.0000} ms");
        }

        // 示例依赖关系
        public class Dependency
        {
            public int Value = 42;
        }

        public class ComponentWithDependency
        {
            public Dependency dep;
            public ComponentWithDependency(Dependency dep)
            {
                this.dep = dep;
            }
        }
    }

}