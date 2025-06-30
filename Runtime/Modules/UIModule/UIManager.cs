using UnityEngine;
using VContainer;

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
        private readonly UIConfig config;
        private readonly UIPoolManager pool;
        private readonly IObjectResolver container;

        private Transform uiParent;
        private Transform dialogParent;


        public UIManager(UIConfig config, UIPoolManager pool, IObjectResolver container)
        {
            this.config = config;
            this.pool = pool;
            this.container = container;
            pool.Init(config.UIKeepAliveTime);
        }

        // 保持原始接口不变（向后兼容）
        public TPresenter OpenUI<TView, TPresenter>(UIType type = UIType.Panel, Transform parent = null)
            where TPresenter : BaseUIPresenter<TView>, new()
            where TView : BaseUIView<TPresenter>
        {
            return OpenUI<TView, TPresenter>(out _, type, parent);
        }

        public TPresenter OpenUI<TView, TPresenter>(out bool isFirstCreate, UIType type = UIType.Panel, Transform parent = null)
        where TPresenter : BaseUIPresenter<TView>, new()
        where TView : BaseUIView<TPresenter>
        {
            // 1. 获取 parent
            parent ??= GetDefaultParent(type);

            // 2. Item 类型不走缓存或复用，直接创建并绑定
            if (type == UIType.Item)
            {
                isFirstCreate = true;
                return CreateUI<TView, TPresenter>(type, parent);
            }

            // 3. 隐藏上一个 UI（只对 Panel 有效）
            if (type == UIType.Panel)
                HideLastUI(parent);

            // 4. 查找已有 UI（先找已挂载，再找对象池）
            var viewObj = UIUtility.FindUI<TView>(parent);
            if (viewObj != null)
            {
                viewObj.SetAsLastSibling();
                UIUtility.TriggerLifetime(viewObj, lifetime => { lifetime.OnShow(); });
                UIUtility.SetUIVisible(viewObj.gameObject, true);
                isFirstCreate = false;
                return viewObj.GetComponent<TView>().presenter;
            }
            else if (pool.TryGetFromPool<TView>(out viewObj))
            {
                viewObj.SetParent(parent);
                viewObj.localPosition = Vector3.zero;
                UIUtility.TriggerLifetime(viewObj, lifetime => { lifetime.OnShow(); });
                UIUtility.SetUIVisible(viewObj.gameObject, true);
                isFirstCreate = false;
                return viewObj.GetComponent<TView>().presenter;
            }
            else
            {
                isFirstCreate = true;
                return CreateUI<TView, TPresenter>(type, parent);
            }
        }

        private TPresenter CreateUI<TView, TPresenter>(UIType type, Transform parent)
        where TPresenter : BaseUIPresenter<TView>, new()
        where TView : BaseUIView<TPresenter>
        {
            var presenter = new TPresenter()
            {
                UIType = type,
                UIParent = parent
            };
            container.Inject(presenter);

            var view = UIUtility.CreateUI<TView>(parent, config.UIPath);
            view.FindComponents();
            view.BindPresenter(presenter);
            view.OnCreate();
            UIUtility.TriggerLifetime(view.obj.transform, lifetime => { lifetime.OnShow(); });
            return presenter;
        }

        public void CloseUI<TView, TPresenter>(UIType type = UIType.Panel, Transform parent = null)
        where TPresenter : BaseUIPresenter<TView>
        where TView : BaseUIView<TPresenter>
        {
            if (type == UIType.Item)
            {
                Debug.LogError($"{type} can not use UIRouter or UIManager close please use BaseUIPresenter Close function");
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
                    UIUtility.TriggerLifetime(lastUI, lifetime => { lifetime.OnShow(); });
                    UIUtility.SetUIVisible(lastUI.gameObject, true);
                }
            }
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
