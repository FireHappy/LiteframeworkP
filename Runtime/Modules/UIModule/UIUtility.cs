using System;
using LiteFramework.Core.MVP;
using UnityEngine;

namespace LiteFramework.Module.UI
{
    internal static class UIUtility
    {
        public static TView CreateUI<TView>(Transform parent, string uiPath, bool setActive = true) where TView : Component, IView
        {
            string prefabName = typeof(TView).Name;
            var go = Resources.Load<GameObject>($"{uiPath}/{prefabName}");
            if (go == null)
            {
                Debug.LogWarning($"找不到 ui 资源：{uiPath}/{prefabName}");
                return default;
            }

            var instance = GameObject.Instantiate(go, parent).transform;
            instance.name = prefabName;
            instance.localScale = Vector3.one;
            instance.gameObject.SetActive(setActive);
            return instance.gameObject.AddComponent<TView>();
        }

        public static void DestroyUI(Transform uiTransform)
        {
#if UNITY_EDITOR
            UnityEngine.Object.DestroyImmediate(uiTransform.gameObject);
#else
            UnityEngine.Object.Destroy(uiTransform.gameObject);
#endif
        }

        public static Transform FindUI<T>(Transform parent)
        {
            return parent.Find(typeof(T).Name);
        }


        public static Transform FindUI(Type viewType, Transform parent)
        {
            return parent.Find(viewType.Name);
        }

        public static void SetUIVisible(GameObject target, bool visible)
        {
            if (target == null) return;

            // 获取或添加 CanvasGroup
            CanvasGroup cg = target.GetComponent<CanvasGroup>();
            if (cg == null)
            {
                cg = target.AddComponent<CanvasGroup>();
            }

            // 设置属性
            cg.alpha = visible ? 1f : 0f;
            cg.interactable = visible;
            cg.blocksRaycasts = visible;
        }
    }
}

