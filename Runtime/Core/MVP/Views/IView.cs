

using UnityEngine;

namespace LiteFramework.Core.MVP
{
    public interface IView
    {
        void BindPresenter(IPresenter presenter);
        void UnBindPresenter();
        GameObject obj { get; }
    }
}

