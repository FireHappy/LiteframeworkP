using System;
using System.Collections.Generic;
using LiteFramework.Core.MVP;
using UnityEngine;
using VContainer.Unity;

namespace LiteFramework.Module.UI
{
    public class UIPoolEntry
    {
        public IView IView;
        public IUILifetime ILifeTime;
        public Transform View;
        public float LastHideTime;

        public UIPoolEntry(Transform view)
        {
            View = view;
            IView = view.GetComponent<IView>();
            ILifeTime = view.GetComponent<IUILifetime>();
            LastHideTime = Time.unscaledTime;
        }
    }


    public class UIPoolManager : ITickable
    {
        private Dictionary<Type, UIPoolEntry> uiPool = new Dictionary<Type, UIPoolEntry>();
        private Transform poolRoot;
        private float keepAliveTime;
        private Queue<Type> removeQueue = new Queue<Type>();

        public UIPoolManager()
        {

        }

        public void Init(float keepAliveTime)
        {
            this.keepAliveTime = keepAliveTime;
            InitPoolRoot();
        }

        public void Tick()
        {
            float now = Time.unscaledTime;
            foreach (var kvp in uiPool)
            {
                if (now - kvp.Value.LastHideTime > keepAliveTime)
                {
                    removeQueue.Enqueue(kvp.Key);
                }
            }
            while (removeQueue.Count > 0)
            {
                Type key = removeQueue.Dequeue();
                if (uiPool.TryGetValue(key, out UIPoolEntry entry))
                {
                    entry.IView.UnBindPresenter();
                    entry.ILifeTime.OnDispose();
                    UIUtility.DestroyUI(entry.View);
                    uiPool.Remove(key);
                }
            }
        }

        public void RecycleUI<TView>(Transform view)
        where TView : IView
        {
            view.transform.SetParent(poolRoot);
            UIPoolEntry entry = new UIPoolEntry(view);
            entry.ILifeTime.OnHide();
            uiPool.Add(typeof(TView), entry);
        }

        public bool TryGetFromPool<TView>(out Transform view)
        {
            if (uiPool.TryGetValue(typeof(TView), out UIPoolEntry entry))
            {
                uiPool.Remove(typeof(TView));
                view = entry.View;
                return true;
            }
            view = null;
            return false;
        }

        private void InitPoolRoot()
        {
            var go = new GameObject("UIPoolRoot");
            var canvasGroup = go.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            poolRoot = go.transform;
            GameObject.DontDestroyOnLoad(go);
        }
    }
}
