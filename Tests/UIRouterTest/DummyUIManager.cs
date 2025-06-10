using System;
using LiteFramework.Core.MVP;
using LiteFramework.Module.UI;
using UnityEngine;


namespace LiteFramework.Tests
{
    public class DummyUIManager : IUIManager
    {
        public void OpenUI<TPresenter, TView>(UIType type = UIType.Panel, Transform parent = null)
            where TPresenter : BaseUIPresenter<TView>
            where TView : BaseUIView<TPresenter>
        {

        }

        public void CloseUI<TPresenter, TView>(UIType type = UIType.Panel, Transform parent = null)
            where TPresenter : BaseUIPresenter<TView>
            where TView : BaseUIView<TPresenter>
        {

        }

        public void OpenUIAsync<TPresenter, TView>(UIType type = UIType.Panel, Transform parent = null, Action success = null, Action<string> failed = null)
            where TPresenter : BaseUIPresenter<TView>
            where TView : BaseUIView<TPresenter>
        {

        }

        public void CloseUIAsync<TPresenter, TView>(UIType type = UIType.Panel, Transform parent = null, Action success = null, Action<string> failed = null)
            where TPresenter : BaseUIPresenter<TView>
            where TView : BaseUIView<TPresenter>
        {

        }
    }
}