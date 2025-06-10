namespace LiteFramework.Module.UI
{
    public interface IUILifetime
    {
        void OnCreate();
        void OnShow();
        void OnHide();
        void OnDispose();
    }
}

