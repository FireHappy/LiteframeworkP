
using System.Diagnostics;
using LiteFramework.Module.UI;
using UnityEngine;


namespace LiteFramework.Tests
{
    public class UIRouterTest : MonoBehaviour
    {
        public int TestIterations = 1_000;

        void Start()
        {
            var manager = new DummyUIManager();
            var router = new UIRouter(manager);     // 使用泛型委托缓存，逆天的牛逼方案
            var routerInvoke = new UIRouterInvoke(manager);
            UIRouter.Register<DummyPresenter, DummyView>();
            UnityEngine.Debug.Log("---- UI Router Performance Test ----");
            Stopwatch sw = new Stopwatch();

            //使用反射的路由方案
            sw.Reset();
            sw.Start();
            for (int i = 0; i < TestIterations; i++)
            {
                routerInvoke.Open<DummyView>();
            }
            sw.Stop();
            UnityEngine.Debug.Log($"UIRouter Reflect Invoke: {sw.ElapsedMilliseconds} ms");

            // 使用泛型缓存的路由方案
            sw.Reset();
            sw.Start();
            for (int i = 0; i < TestIterations; i++)
            {
                router.Open<DummyView>();
            }
            sw.Stop();
            UnityEngine.Debug.Log($"UIRouter Type Cache Invoke: {sw.ElapsedMilliseconds} ms");

            //静态调用
            sw.Reset();
            sw.Start();
            for (int i = 0; i < TestIterations; i++)
            {
                manager.OpenUI<DummyPresenter, DummyView>();
            }
            sw.Stop();
            UnityEngine.Debug.Log($"Static Invoke : {sw.ElapsedMilliseconds} ms");
        }
    }

}