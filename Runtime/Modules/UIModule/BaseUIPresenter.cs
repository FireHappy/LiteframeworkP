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
        private GameObject viewObj;
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
                //TODO 处理item的销毁
                // IUILifetime[] lifetimes = viewObj.GetComponentsInChildren<IUILifetime>();
                // for (int i = 0; i < lifetimes.Length; i++)
                // {
                //     lifetimes[i].OnHide();
                // }
                // for (int i = 0; i < lifetimes.Length; i++)
                // {
                //     lifetimes[i].OnDispose();
                // }
                // viewObj.GetComponent<TView>().UnBindPresenter();
                GameObject.Destroy(viewObj);
                return;
            }
            router.Close<TView>(UIType, UIParent);
        }
    }
}

