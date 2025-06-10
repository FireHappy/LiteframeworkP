using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;


namespace LiteFramework.Tests
{
    public class DelegateVsReflectionTest : MonoBehaviour
    {
        public enum UIType { Panel }

        public class DummyUIManager
        {
            public void OpenUI<T1, T2>(UIType type, Transform parent)
            {
                // 模拟方法体
            }
        }

        private Action<UIType, Transform> _cachedDelegate;
        private MethodInfo _cachedMethod;
        private DummyUIManager _manager;

        void Start()
        {
            _manager = new DummyUIManager();

            // 缓存反射方法
            _cachedMethod = typeof(DummyUIManager).GetMethod("OpenUI")!
                .MakeGenericMethod(typeof(string), typeof(int));

            // 缓存委托（注意：不捕获 uiManager，手动绑定参数）
            var param1 = Expression.Parameter(typeof(UIType), "type");
            var param2 = Expression.Parameter(typeof(Transform), "parent");

            var call = Expression.Call(
                Expression.Constant(_manager),  // 没有闭包
                _cachedMethod,
                param1,
                param2
            );

            var lambda = Expression.Lambda<Action<UIType, Transform>>(call, param1, param2);
            _cachedDelegate = lambda.Compile();

            RunBenchmark();
        }

        void RunBenchmark()
        {
            const int Iterations = 1_000_000;

            // Warmup
            _cachedDelegate(UIType.Panel, null);
            _cachedMethod.Invoke(_manager, new object[] { UIType.Panel, null });

            // Delegate Benchmark
            var sw1 = Stopwatch.StartNew();
            for (int i = 0; i < Iterations; i++)
            {
                _cachedDelegate(UIType.Panel, null);
            }
            sw1.Stop();

            // Reflection Benchmark
            var sw2 = Stopwatch.StartNew();
            for (int i = 0; i < Iterations; i++)
            {
                _cachedMethod.Invoke(_manager, new object[] { UIType.Panel, null });
            }
            sw2.Stop();

            UnityEngine.Debug.Log($"[Delegate]    {sw1.ElapsedMilliseconds} ms");
            UnityEngine.Debug.Log($"[Reflection]  {sw2.ElapsedMilliseconds} ms");
        }
    }

}