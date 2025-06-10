using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using LiteFramework.Core.MVP;
using LiteFramework.Module.UI;

namespace LiteFramework.Tests
{
    public class UIRouterInvoke
    {
        private readonly IUIManager uiManager;

        private static readonly Dictionary<Type, Type> PresenterTypeCache = new();

        private static readonly Dictionary<(Type presenter, Type view), Action<IUIManager, UIType, Transform>> OpenDelegates = new();
        private static readonly Dictionary<(Type presenter, Type view), Action<IUIManager, UIType, Transform>> CloseDelegates = new();


        public UIRouterInvoke(IUIManager uiManager)
        {
            this.uiManager = uiManager;
        }

        public void Open<TView>(UIType type = UIType.Panel, Transform parent = null)
            where TView : IView
        {
            var presenterType = GetPresenterTypeFromView<TView>();
            if (presenterType == null)
            {
                Debug.LogError($"UIRouter.Open -> Failed to resolve Presenter type for view {typeof(TView).Name}");
            }

            var openDel = GetOpenDelegate(presenterType, typeof(TView));
            openDel(type, parent);
        }

        public void Close<TView>(UIType type = UIType.Panel, Transform parent = null)
            where TView : IView
        {
            var presenterType = GetPresenterTypeFromView<TView>();
            if (presenterType == null)
            {
                Debug.LogError($"UIRouter.Close -> Failed to resolve Presenter type for view {typeof(TView).Name}");
                return;
            }

            var closeDel = GetCloseDelegate(presenterType, typeof(TView));
            closeDel(type, parent);
        }

        private Type GetPresenterTypeFromView<TView>() where TView : IView
        {
            var viewType = typeof(TView);

            if (PresenterTypeCache.TryGetValue(viewType, out var cachedType))
                return cachedType;

            var baseType = viewType.BaseType;
            while (baseType != null)
            {
                if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(BaseUIView<>))
                {
                    var presenterType = baseType.GetGenericArguments()[0];
                    PresenterTypeCache[viewType] = presenterType;
                    return presenterType;
                }

                baseType = baseType.BaseType;
            }

            return null;
        }


        private Action<UIType, Transform> GetOpenDelegate(Type presenterType, Type viewType)
        {
            var key = (presenterType, viewType);
            if (OpenDelegates.TryGetValue(key, out var compiled))
                return (t, p) => compiled(uiManager, t, p);  // 闭包只有一次

            var method = typeof(IUIManager).GetMethod("OpenUI");
            var genericMethod = method.MakeGenericMethod(presenterType, viewType);

            var paramInstance = Expression.Parameter(typeof(IUIManager), "instance");
            var paramType = Expression.Parameter(typeof(UIType), "type");
            var paramParent = Expression.Parameter(typeof(Transform), "parent");

            var call = Expression.Call(paramInstance, genericMethod, paramType, paramParent);

            var lambda = Expression.Lambda<Action<IUIManager, UIType, Transform>>(call, paramInstance, paramType, paramParent);

            compiled = lambda.Compile();
            OpenDelegates[key] = compiled;

            // 返回封装的委托，只捕获一次 uiManager
            return (t, p) => compiled(uiManager, t, p);
        }


        private Action<UIType, Transform> GetCloseDelegate(Type presenterType, Type viewType)
        {
            var key = (presenterType, viewType);
            if (CloseDelegates.TryGetValue(key, out var compiled))
                return (t, p) => compiled(uiManager, t, p); // ✅ 仅捕获一次 uiManager

            var method = typeof(IUIManager).GetMethod("CloseUI");
            var genericMethod = method.MakeGenericMethod(presenterType, viewType);

            var paramInstance = Expression.Parameter(typeof(IUIManager), "instance");
            var paramType = Expression.Parameter(typeof(UIType), "type");
            var paramParent = Expression.Parameter(typeof(Transform), "parent");

            var call = Expression.Call(paramInstance, genericMethod, paramType, paramParent);

            var lambda = Expression.Lambda<Action<IUIManager, UIType, Transform>>(call, paramInstance, paramType, paramParent);

            compiled = lambda.Compile();
            CloseDelegates[key] = compiled;

            return (t, p) => compiled(uiManager, t, p); // ✅ 闭包只捕获一次
        }

    }
}
