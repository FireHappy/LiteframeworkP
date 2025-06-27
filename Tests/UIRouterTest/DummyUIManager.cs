using System;
using LiteFramework.Core.MVP;
using LiteFramework.Module.UI;
using UnityEngine;


namespace LiteFramework.Tests
{
    public class DummyUIManager : IUIManager
    {
        public void CloseUI<TView, TPresenter>(UIType type = UIType.Panel, Transform parent = null)
            where TView : BaseUIView<TPresenter>
            where TPresenter : BaseUIPresenter<TView>
        {

        }

        public void CloseUIAsync<TView, TPresenter>(UIType type = UIType.Panel, Transform parent = null, Action success = null, Action<string> failed = null)
            where TView : BaseUIView<TPresenter>
            where TPresenter : BaseUIPresenter<TView>
        {

        }

        public TPresenter OpenUI<TView, TPresenter>(UIType type = UIType.Panel, Transform parent = null)
            where TView : BaseUIView<TPresenter>
            where TPresenter : BaseUIPresenter<TView>
        {
            return default;
        }

        public TPresenter OpenUIAsync<TView, TPresenter>(UIType type = UIType.Panel, Transform parent = null, Action success = null, Action<string> failed = null)
            where TView : BaseUIView<TPresenter>
            where TPresenter : BaseUIPresenter<TView>
        {
            return default;
        }
    }
}