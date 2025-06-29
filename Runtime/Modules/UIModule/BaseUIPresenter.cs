using LiteFramework.Core.MVP;
using UnityEngine;
using VContainer;

namespace LiteFramework.Module.UI
{
    public abstract class BaseUIPresenter<TView> : IPresenter
        where TView : IView
    {
        protected TView view;
        protected readonly IObjectResolver container;

        protected readonly UIRouter router;
        private GameObject viewObj;
        private UIType uiType = UIType.Panel;

        public UIType UIType
        {
            get
            {
                return uiType;
            }
            set
            {
                uiType = value;
            }
        }

        private IScopedObjectResolver scope;
        public IScopedObjectResolver Scope
        {
            get
            {
                return scope;
            }
            set
            {
                scope = value;
            }
        }

        private Transform uiParent;
        public Transform UIParent
        {
            get
            {
                return uiParent;
            }
            set
            {
                uiParent = value;
            }
        }
        public BaseUIPresenter()
        {
        }
        protected BaseUIPresenter(UIRouter router, IObjectResolver container)
        {
            this.container = container;
            this.router = router;
        }

        public void AttachView(IView view)
        {
            this.view = (TView)view;
            this.viewObj = view.obj;
        }

        public void DetachView()
        {
            view = default;
        }

        public virtual void OnViewCreate() { }
        public virtual void OnViewShow() { }
        public virtual void OnViewHide() { }
        public virtual void OnViewDispose()
        {
            scope?.Dispose();
        }

        public void Close()
        {
            if (uiType == UIType.Item)
            {
                UIUtility.TriggerLifetime(viewObj.transform, lifetime =>
                {
                    lifetime.OnHide();
                    lifetime.OnDispose();
                });
                viewObj.GetComponent<TView>().UnBindPresenter();
                GameObject.Destroy(viewObj);
                return;
            }
            router.Close<TView>(UIType, UIParent);
        }
    }
}

