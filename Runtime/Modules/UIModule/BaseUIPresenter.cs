using LiteFramework.Core.MVP;
using VContainer;

namespace LiteFramework.Module.UI
{
    public abstract class BaseUIPresenter<TView> : IPresenter
        where TView : IView
    {
        protected TView view;
        protected readonly IObjectResolver container;
        protected readonly UIRouter router;

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
    }
}

