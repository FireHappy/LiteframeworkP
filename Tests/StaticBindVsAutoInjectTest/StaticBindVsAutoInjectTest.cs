using System;
using System.Collections;
using System.Collections.Generic;
using LiteFramework.Core.Utility;
using UnityEngine;


namespace LiteFramework.Tests
{
    public class StaticBindVsAutoInjectTest : MonoBehaviour
    {
        public Transform UIRoot;
        public MyUIView Target;

        public int LoopCount = 10000;

        void Start()
        {
            // Warmup
            AutoInjectComponent.AutoInject(UIRoot, Target);
            Target.StaticBind(UIRoot);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();


            // Test Reflection Inject
            long beforeMemory1 = GC.GetTotalMemory(true);
            var sw1 = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < LoopCount; i++)
            {
                Target.Clear();
                AutoInjectComponent.AutoInject(UIRoot, Target);
            }
            sw1.Stop();
            long afterMemory1 = GC.GetTotalMemory(false);
            Debug.Log($"[反射注入] Time: {sw1.Elapsed.TotalMilliseconds:F2} ms, GC Alloc: {afterMemory1 - beforeMemory1} bytes");

            // Test Static Binding
            long beforeMemory2 = GC.GetTotalMemory(true);
            var sw2 = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < LoopCount; i++)
            {
                Target.Clear();
                Target.StaticBind(UIRoot);
            }
            sw2.Stop();
            long afterMemory2 = GC.GetTotalMemory(false);
            Debug.Log($"[静态绑定] Time: {sw2.Elapsed.TotalMilliseconds:F2} ms, GC Alloc: {afterMemory2 - beforeMemory2} bytes");
        }
    }

}