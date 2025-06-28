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

    // UI实例包装器，统一管理 View 和 Presenter 的生命周期
    public class UIInstance<TView, TPresenter> : IDisposable
        where TPresenter : BaseUIPresenter<TView>
        where TView : BaseUIView<TPresenter>
    {
        public TView View { get; private set; }
        public TPresenter Presenter { get; private set; }
        public LifetimeScope Scope { get; private set; } // 新增：作用域引用
        public UIType UIType { get; private set; }
        public bool IsActive { get; private set; }

        public UIInstance(TView view, TPresenter presenter, LifetimeScope scope, UIType uiType)
        {
            View = view;
            Presenter = presenter;
            Scope = scope; // 存储作用域引用
            UIType = uiType;
            IsActive = true;
        }

        public void Show()
        {
            if (!IsActive) return;

            UIUtility.SetUIVisible(View.gameObject, true);
            View.gameObject.transform.SetAsLastSibling();

            // 触发生命周期
            var lifetimes = View.GetComponentsInChildren<IUILifetime>();
            foreach (var lifetime in lifetimes)
            {
                lifetime.OnShow();
            }
        }

        public void Hide()
        {
            if (!IsActive) return;

            UIUtility.SetUIVisible(View.gameObject, false);

            // 触发生命周期
            if (View.TryGetComponent<IUILifetime>(out var lifetime))
            {
                lifetime.OnHide();
            }
        }

        public void Dispose()
        {
            if (!IsActive) return;

            IsActive = false;

            // 解除绑定
            View?.UnBindPresenter();

            // 释放作用域（关键修改）
            Scope?.Dispose();
            Scope = null;

            // 销毁 View GameObject
            if (View != null && View.gameObject != null)
            {
                UnityEngine.Object.Destroy(View.gameObject);
            }

            View = null;
            Presenter = null;
        }
    }

    // UI工厂
    public interface IUIFactory
    {
        UIInstance<TView, TPresenter> CreateUI<TView, TPresenter>(UIType type, Transform parent)
            where TPresenter : BaseUIPresenter<TView>
            where TView : BaseUIView<TPresenter>;
    }

    public class UIFactory : IUIFactory
    {
        private readonly IObjectResolver container;
        private readonly UIConfig config;

        public UIFactory(IObjectResolver container, UIConfig config)
        {
            this.container = container;
            this.config = config;
        }

        public UIInstance<TView, TPresenter> CreateUI<TView, TPresenter>(UIType type, Transform parent)
            where TPresenter : BaseUIPresenter<TView>
            where TView : BaseUIView<TPresenter>
        {
            // 1. 创建作用域（关键修改）
            var scope = container.CreateScope(builder =>
            {
                // 注册Presenter到子作用域
                builder.Register<TPresenter>(Lifetime.Scoped);
            });

            // 2. 从作用域解析Presenter
            var presenter = scope.Container.Resolve<TPresenter>();
            presenter.UIType = type;
            presenter.UIParent = parent;

            // 3. 创建 View
            var view = UIUtility.CreateUI<TView>(parent, config.UIPath);

            // 4. 注入依赖到View（关键修改）
            scope.Container.InjectGameObject(view.gameObject);

            // 5. 绑定关系
            view.FindComponents();
            view.BindPresenter(presenter);
            view.OnCreate();

            // 6. 存储作用域引用到Presenter（可选）
            presenter.SetScope(scope);

            // 7. 返回 UI 实例（传入作用域）
            return new UIInstance<TView, TPresenter>(view, presenter, scope, type);
        }
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
            if (type == UIType.Item)
            {
                return;
            }
            parent ??= GetDefaultParent(type);
            var view = UIUtility.FindUI<TView>(parent);
            if (view != null)
            {
                //Recycle UI To Pool
                pool.RecycleUI<TView>(view);
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
