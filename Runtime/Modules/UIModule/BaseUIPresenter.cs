using LiteFramework.Core.MVP;
using UnityEngine;
using VContainer;

namespace LiteFramework.Module.UI
{
    public abstract class BaseUIPresenter<TView> : IPresenter
        where TView : IView
    {
        [Inject]
        protected readonly IObjectResolver container;
        [Inject]
        protected readonly UIRouter router;
        protected TView view;
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
        public virtual void OnViewDispose() { }

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
                UIUtility.DestroyUI(viewObj.transform);
                return;
            }
            router.Close<TView>(UIType, UIParent);
        }
    }
}

