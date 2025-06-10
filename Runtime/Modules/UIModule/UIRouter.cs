using System;
using UnityEngine;
using LiteFramework.Core.MVP;

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
            where TPresenter : BaseUIPresenter<TView>
            where TView : BaseUIView<TPresenter>
        {
            // 使用强类型委托避免反射,使用泛型的特点，避免字典查找
            ViewCache<TView>.OpenAction = (IUIManager mgr, UIType type, Transform parent) =>
                mgr.OpenUI<TPresenter, TView>(type, parent);
            ViewCache<TView>.CloseAction = (IUIManager mgr, UIType type, Transform parent) =>
                mgr.CloseUI<TPresenter, TView>(type, parent);
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

        public void Open<TPresenter, TView>(UIType type = UIType.Panel, Transform parent = null)
            where TPresenter : BaseUIPresenter<TView>
            where TView : BaseUIView<TPresenter>
        {
            uiManager.OpenUI<TPresenter, TView>(type, parent);
        }

        public void Close<TPresenter, TView>(UIType type = UIType.Panel, Transform parent = null)
            where TPresenter : BaseUIPresenter<TView>
            where TView : BaseUIView<TPresenter>
        {
            uiManager.CloseUI<TPresenter, TView>(type, parent);
        }
    }
}
