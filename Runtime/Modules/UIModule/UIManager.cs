using UnityEngine;
using VContainer;
using System;

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

        public TPresenter OpenUI<TView, TPresenter>(UIType type = UIType.Panel, Transform parent = null)
        where TPresenter : BaseUIPresenter<TView>
        where TView : BaseUIView<TPresenter>
        {
            // 1. 获取 parent
            parent ??= GetDefaultParent(type);

            // 2. 隐藏上一个 UI（只对 Panel 有效）
            if (type == UIType.Panel)
                HideLastUI(parent);

            // 3. 创建 Presenter
            var presenter = container.Resolve<TPresenter>();
            presenter.UIType = type;
            presenter.UIParent = parent;

            // 4. Item 类型不走缓存或复用，直接创建并绑定
            if (type == UIType.Item)
            {
                CreateAndBindView<TView, TPresenter>(presenter, parent);
                return presenter;
            }

            // 5. 查找已有 UI（先找已挂载，再找对象池）
            var viewObj = UIUtility.FindUI<TView>(parent);

            if (viewObj != null)
            {
                viewObj.SetAsLastSibling();
                UIUtility.SetUIVisible(viewObj.gameObject, true);
            }
            else if (pool.TryGetFromPool<TView>(out viewObj))
            {
                viewObj.SetParent(parent);
                viewObj.localPosition = Vector3.zero;
                IUILifetime[] lifetimes = viewObj.GetComponentsInChildren<IUILifetime>();
                for (int i = 0; i < lifetimes.Length; i++)
                {
                    lifetimes[i].OnShow();
                }
                UIUtility.SetUIVisible(viewObj.gameObject, true);
            }
            else
            {
                CreateAndBindView<TView, TPresenter>(presenter, parent);
            }

            return presenter;
        }

        private Transform GetDefaultParent(UIType type)
        {
            return type switch
            {
                UIType.Panel => GetUIParent(),
                UIType.Dialog => GetDialogParent(),
                UIType.Item => GetUIParent(),
                _ => GetUIParent(),
            };
        }

        private void HideLastUI(Transform parent)
        {
            var lastUI = GetTopChild(parent);
            if (lastUI != null && lastUI.TryGetComponent<IUILifetime>(out var life))
            {
                life.OnHide();
                UIUtility.SetUIVisible(lastUI.gameObject, false);
            }
        }

        private TView CreateAndBindView<TView, TPresenter>(TPresenter presenter, Transform parent)
            where TPresenter : BaseUIPresenter<TView>
            where TView : BaseUIView<TPresenter>
        {
            TView view = UIUtility.CreateUI<TView>(parent, config.UIPath);
            view.FindComponents();
            view.BindPresenter(presenter);
            view.OnCreate();
            return view;
        }


        public void CloseUI<TView, TPresenter>(UIType type = UIType.Panel, Transform parent = null)
        where TPresenter : BaseUIPresenter<TView>
        where TView : BaseUIView<TPresenter>
        {
            parent ??= GetDefaultParent(type);
            var view = UIUtility.FindUI<TView>(parent);
            if (view != null)
            {
                if (type == UIType.Item)
                {
                    IUILifetime[] lifetimes = view.GetComponentsInChildren<IUILifetime>();
                    for (int i = 0; i < lifetimes.Length; i++)
                    {
                        lifetimes[i].OnDispose();
                    }
                    view.GetComponent<TView>().UnBindPresenter();
                    UIUtility.DestroyUI(view);
                    return;
                }
                else
                {
                    //Recycle UI To Pool
                    pool.RecycleUI<TView>(view);
                }
            }
            if (type == UIType.Panel)
            {
                var lastUI = GetTopChild(parent);
                if (lastUI != null)
                {
                    IUILifetime[] lifetimes = lastUI.GetComponentsInChildren<IUILifetime>();
                    for (int i = 0; i < lifetimes.Length; i++)
                    {
                        lifetimes[i].OnShow();
                    }
                    UIUtility.SetUIVisible(lastUI.gameObject, true);
                }
            }
        }


        public TPresenter OpenUIAsync<TView, TPresenter>(UIType type = UIType.Panel, Transform parent = null, Action success = null, Action<string> failed = null)
                   where TPresenter : BaseUIPresenter<TView>
                   where TView : BaseUIView<TPresenter>
        {
            //todo 实现UI的异步加载
            return default;
        }

        public void CloseUIAsync<TView, TPresenter>(UIType type = UIType.Panel, Transform parent = null, Action success = null, Action<string> failed = null)
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
