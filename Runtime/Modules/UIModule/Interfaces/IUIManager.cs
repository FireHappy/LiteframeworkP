using System;
using UnityEngine;

namespace LiteFramework.Module.UI
{
    public interface IUIManager
    {
        public TPresenter OpenUI<TView, TPresenter>(UIType type = UIType.Panel, Transform parent = null)
            where TPresenter : BaseUIPresenter<TView>
            where TView : BaseUIView<TPresenter>;

        public void CloseUI<TView, TPresenter>(UIType type = UIType.Panel, Transform parent = null) where TPresenter : BaseUIPresenter<TView>
            where TView : BaseUIView<TPresenter>;


        public TPresenter OpenUIAsync<TView, TPresenter>(UIType type = UIType.Panel, Transform parent = null, Action success = null, Action<string> failed = null)
            where TPresenter : BaseUIPresenter<TView>
            where TView : BaseUIView<TPresenter>;


        public void CloseUIAsync<TView, TPresenter>(UIType type = UIType.Panel, Transform parent = null, Action success = null, Action<string> failed = null) where TPresenter : BaseUIPresenter<TView>
            where TView : BaseUIView<TPresenter>;
    }
}
