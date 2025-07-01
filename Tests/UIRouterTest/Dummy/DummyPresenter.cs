using LiteFramework.Core.Utility;
using LiteFramework.Core.MVP;
using LiteFramework.Module.UI;
using VContainer;
using UnityEngine;

namespace LiteFramework.Tests
{
    [AutoRegister(VContainer.Lifetime.Transient)]
    public class DummyPresenter : BaseUIPresenter<DummyView>
    {
        public DummyPresenter() { }

        public override void OnViewCreate()
        {

        }

        public override void OnViewDispose()
        {

        }
    }
}


