using System;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
using LiteFramework.Core.Utility;


namespace LiteFramework.Tests
{
    public class AutoInjectTest : MonoBehaviour
    {
        public int TestCount = 10000;

        class DummyComponent : MonoBehaviour { }

        class TestTarget
        {
            [Autowrited("dummy")]
            public GameObject DummyGO;

            [Autowrited("dummy")]
            public DummyComponent DummyComp;
        }

        void Start()
        {
            RunPerformanceTest();
        }

        void RunPerformanceTest()
        {
            // 创建测试对象
            var dummyGO = new GameObject("dummy");
            var dummyComp = dummyGO.AddComponent<DummyComponent>();

            var root = dummyGO.transform;

            // 原始反射
            var watch1 = Stopwatch.StartNew();
            for (int i = 0; i < TestCount; ++i)
            {
                var target = new TestTarget();
                AutoInjectComponent.AutoInject(root, target);
            }
            watch1.Stop();

            // 表达式树
            var watch2 = Stopwatch.StartNew();
            for (int i = 0; i < TestCount; ++i)
            {
                var target = new TestTarget();
                AutoInjectComponentExpression.AutoInject(root, target);
            }
            watch2.Stop();

            Debug.Log($"反射 SetValue 耗时: {watch1.ElapsedMilliseconds} ms");
            Debug.Log($"表达式树委托 耗时: {watch2.ElapsedMilliseconds} ms");
        }
    }

}