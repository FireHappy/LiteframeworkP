
namespace LiteFramework.Core.MVP
{
    public interface IPresenter
    {
        void AttachView(IView view);
        void DetachView();
    }
}
