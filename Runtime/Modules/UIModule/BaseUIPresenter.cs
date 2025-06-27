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

        protected BaseUIPresenter(UIRouter router, IObjectResolver container)
        {
            this.container = container;
            this.router = router;
        }

        public void AttachView(IView view)
        {
            this.view = (TView)view;
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
            router.Close<TView>(UIType, UIParent);
        }
    }
}

