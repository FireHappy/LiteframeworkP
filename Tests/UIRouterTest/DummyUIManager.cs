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


        public TPresenter OpenUI<TView, TPresenter>(UIType type = UIType.Panel, Transform parent = null)
            where TView : BaseUIView<TPresenter>
            where TPresenter : BaseUIPresenter<TView>, new()
        {
            return default;
        }

        public TPresenter OpenUI<TView, TPresenter>(out bool isFirstCreate, UIType type = UIType.Panel, Transform parent = null)
            where TView : BaseUIView<TPresenter>
            where TPresenter : BaseUIPresenter<TView>, new()
        {
            isFirstCreate = false;
            return default;
        }
    }
}