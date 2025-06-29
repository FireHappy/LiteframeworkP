using System;
using UnityEngine;
using LiteFramework.Core.MVP;
using UnityEditor;

namespace LiteFramework.Module.UI
{
    public class UIRouter
    {
        private readonly IUIManager uiManager;

        private static class ViewCache<TView> where TView : IView
        {
            public static Action<IUIManager, UIType, Transform>? OpenAction;
            public static Action<IUIManager, UIType, Transform>? CloseAction;
        }

        public UIRouter(IUIManager uiManager)
        {
            this.uiManager = uiManager;
        }


        public static void Register<TPresenter, TView>()
            where TPresenter : BaseUIPresenter<TView>, new()
            where TView : BaseUIView<TPresenter>
        {
            // 使用强类型委托避免反射,使用泛型的特点，避免字典查找
            ViewCache<TView>.OpenAction = (IUIManager mgr, UIType type, Transform parent) =>
                mgr.OpenUI<TView, TPresenter>(type, parent);
            ViewCache<TView>.CloseAction = (IUIManager mgr, UIType type, Transform parent) =>
                mgr.CloseUI<TView, TPresenter>(type, parent);
        }

        public void Open<TView>(UIType type = UIType.Panel, Transform parent = null)
            where TView : IView
        {
            var action = ViewCache<TView>.OpenAction;
            if (action == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogError($"UIRouter.Open: {typeof(TView).Name} not registered!");
#endif
                return;
            }
            action(uiManager, type, parent);
        }

        public void Close<TView>(UIType type = UIType.Panel, Transform parent = null)
            where TView : IView
        {
            var action = ViewCache<TView>.CloseAction;
            if (action == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogError($"UIRouter.Close: {typeof(TView).Name} not registered!");
#endif
                return;
            }
            action(uiManager, type, parent);
        }

        public TPresenter Open<TView, TPresenter>(UIType type = UIType.Panel, Transform parent = null)
            where TPresenter : BaseUIPresenter<TView>, new()
            where TView : BaseUIView<TPresenter>
        {
            return uiManager.OpenUI<TView, TPresenter>(type, parent);
        }

        public void Close<TView, TPresenter>(UIType type = UIType.Panel, Transform parent = null)
            where TPresenter : BaseUIPresenter<TView>
            where TView : BaseUIView<TPresenter>
        {
            uiManager.CloseUI<TView, TPresenter>(type, parent);
        }

    }
}
