using System;
using LiteFramework.Core.MVP;
using LiteFramework.Module.UI;
using UnityEngine;


namespace LiteFramework.Tests
{
    public class DummyUIManager : IUIManager
    {
        public TPresenter OpenUI<TPresenter, TView>(UIType type = UIType.Panel, Transform parent = null)
            where TPresenter : BaseUIPresenter<TView>
            where TView : BaseUIView<TPresenter>
        {
            return default;
        }

        public void CloseUI<TPresenter, TView>(UIType type = UIType.Panel, Transform parent = null)
            where TPresenter : BaseUIPresenter<TView>
            where TView : BaseUIView<TPresenter>
        {

        }

        public TPresenter OpenUIAsync<TPresenter, TView>(UIType type = UIType.Panel, Transform parent = null, Action success = null, Action<string> failed = null)
            where TPresenter : BaseUIPresenter<TView>
            where TView : BaseUIView<TPresenter>
        {
            return default;
        }

        public void CloseUIAsync<TPresenter, TView>(UIType type = UIType.Panel, Transform parent = null, Action success = null, Action<string> failed = null)
            where TPresenter : BaseUIPresenter<TView>
            where TView : BaseUIView<TPresenter>
        {

        }
    }
}