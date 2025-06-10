using UnityEngine;
using VContainer;
using System;
using LiteFramework.Configs;

namespace LiteFramework.Module.UI
{
    public enum UIType
    {
        Panel,
        Item,
        Dialog
    }

    public class UIManager : IUIManager
    {

        private readonly IObjectResolver container;
        private readonly UIConfig config;
        private readonly UIPoolManager pool;
        private Transform uiParent;
        private Transform dialogParent;

        public UIManager(IObjectResolver container, UIConfig config, UIPoolManager pool)
        {
            this.container = container;
            this.config = config;
            this.pool = pool;
            pool.Init(config.UIKeepAliveTime);
        }

        public void OpenUI<TPresenter, TView>(UIType type = UIType.Panel, Transform parent = null)
        where TPresenter : BaseUIPresenter<TView>
        where TView : BaseUIView<TPresenter>
        {
            switch (type)
            {
                case UIType.Panel:
                    parent ??= GetUIParent();
                    var lastUI = GetTopChild(parent);
                    if (lastUI != null)
                    {
                        lastUI.GetComponent<IUILifetime>().OnHide();
                        UIUtility.SetUIVisible(lastUI.gameObject, false);
                    }
                    break;
                case UIType.Dialog:
                    parent ??= GetDialogParent();
                    break;
                case UIType.Item:
                    parent ??= GetUIParent();
                    break;
            }
            var viewObj = UIUtility.FindUI<TView>(parent);
            if (viewObj != null)
            {
                viewObj.SetAsLastSibling();
                viewObj.gameObject.SetActive(true);
            }
            else if (pool.TryGetFromPool<TView>(out viewObj))
            {
                viewObj.SetParent(parent);
                viewObj.localPosition = Vector3.zero;
                UIUtility.SetUIVisible(viewObj.gameObject, true);
            }
            else
            {
                TView view = UIUtility.CreateUI<TView>(parent, config.UIPath);
                //查找初始化组件
                view.FindComponents();
                var presenter = container.Resolve<TPresenter>();
                view.BindPresenter(presenter);
                view.OnCreate();
            }
        }

        public void CloseUI<TPresenter, TView>(UIType type = UIType.Panel, Transform parent = null)
        where TPresenter : BaseUIPresenter<TView>
        where TView : BaseUIView<TPresenter>
        {
            switch (type)
            {
                case UIType.Panel:
                    parent ??= GetUIParent();
                    break;
                case UIType.Dialog:
                    parent ??= GetDialogParent();
                    break;
                case UIType.Item:
                    parent ??= GetUIParent();
                    break;
            }
            var tsf = UIUtility.FindUI<TView>(parent);
            if (tsf != null)
            {
                //回收到UI池中
                pool.RecycleUI<TView>(tsf);
            }
            if (type == UIType.Panel)
            {
                var lastUI = GetTopChild(parent);
                if (lastUI != null)
                {
                    lastUI.GetComponent<IUILifetime>().OnShow();
                    UIUtility.SetUIVisible(lastUI.gameObject, true);
                }
            }
        }


        public void OpenUIAsync<TPresenter, TView>(UIType type = UIType.Panel, Transform parent = null, Action success = null, Action<string> failed = null)
                   where TPresenter : BaseUIPresenter<TView>
                   where TView : BaseUIView<TPresenter>
        {
            //todo 实现UI的异步加载
        }

        public void CloseUIAsync<TPresenter, TView>(UIType type = UIType.Panel, Transform parent = null, Action success = null, Action<string> failed = null)
            where TPresenter : BaseUIPresenter<TView>
            where TView : BaseUIView<TPresenter>
        {
            //todo 实现UI的异步销毁
        }

        private Transform GetDialogParent()
        {
            if (dialogParent == null)
            {
                dialogParent = GameObject.FindWithTag(config.DefaultUIDialogTag)?.transform;
            }
            if (dialogParent == null && config.RootUIPrefab != null)
            {
                GameObject.Instantiate(config.RootUIPrefab);
                dialogParent = GameObject.FindWithTag(config.DefaultUIDialogTag)?.transform;
            }
            return dialogParent;
        }

        private Transform GetUIParent()
        {
            if (uiParent == null)
            {
                uiParent = GameObject.FindWithTag(config.DefaultUIParentTag)?.transform;
            }
            if (uiParent == null && config.RootUIPrefab != null)
            {
                GameObject.Instantiate(config.RootUIPrefab);
                uiParent = GameObject.FindWithTag(config.DefaultUIParentTag)?.transform;
            }
            return uiParent;
        }

        private Transform GetTopChild(Transform tsf)
        {
            if (tsf.childCount > 0)
            {
                return tsf.GetChild(tsf.childCount - 1);
            }
            return null;
        }
    }
}
