using VContainer;

namespace LiteFramework.Core.MVP
{
    public abstract class BasePresenter<TView> : IPresenter
        where TView : IView
    {
        protected TView View;
        protected readonly IObjectResolver Container;

        protected BasePresenter(IObjectResolver container)
        {
            Container = container;
        }

        public void AttachView(IView view)
        {
            View = (TView)view;
            OnViewReady();
        }

        public void DetachView()
        {
            View = default;
            OnViewDispose();
        }

        protected virtual void OnViewReady() { }
        protected virtual void OnViewDispose() { }
    }
}

