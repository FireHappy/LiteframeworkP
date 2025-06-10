using UnityEngine;
using VContainer;
using VContainer.Unity;
using LiteFramework.Module.UI;
using LiteFramework.Core.MVP;
using LiteFramework.Configs;
using System.Linq;
using System.Reflection;
using LiteFramework.Core.Utility;

namespace LiteFramework
{
    /// <summary>
    /// 程序启动基类，封装 UI 初始化与自动注册逻辑
    /// </summary>
    public abstract class LiteStartupBase : LifetimeScope
    {
        [SerializeField]
        protected ScriptableObject UIConfig;

        protected override void Configure(IContainerBuilder builder)
        {
            RegisterUIModule(builder);
            RegisterAllAutoRegister(builder);
            OnRegisterCustomServices(builder);
        }

        /// <summary>
        /// 注册 UI 管理模块（UIRootConfig / IUIManager / UIRouter）
        /// </summary>
        private void RegisterUIModule(IContainerBuilder builder)
        {
            if (UIConfig != null)
            {
                builder.RegisterInstance(UIConfig).As<UIConfig>();
                builder.RegisterEntryPoint<UIPoolManager>(Lifetime.Singleton).AsSelf();
                builder.Register<IUIManager, UIManager>(Lifetime.Singleton);
                builder.Register<UIRouter>(Lifetime.Singleton);
            }
        }

        /// <summary>
        /// 自动注册所有带有特性标记的类
        /// </summary>
        private void RegisterAllAutoRegister(IContainerBuilder builder)
        {
            var assemblies = GetAutoRegisterAssemblies();
            VContainerAutoRegister.RegisterWithAttribute(builder, assemblies);
        }

        /// <summary>
        /// 自动注册所需程序集 = 默认程序集 + 子类指定程序集
        /// </summary>
        protected virtual Assembly[] GetAutoRegisterAssemblies()
        {
            var baseAssembly = typeof(BasePresenter<>).Assembly;
            var custom = GetCustomAutoRegisterAssemblies();
            return custom != null
                ? new[] { baseAssembly }.Concat(custom).ToArray()
                : new[] { baseAssembly };
        }

        /// <summary>
        /// 子类可重写此方法添加自动注册程序集
        /// </summary>
        protected virtual Assembly[] GetCustomAutoRegisterAssemblies()
        {
            {
                return new Assembly[] {
                    Assembly.Load("Assembly-CSharp")
                };
            }
        }

        /// <summary>
        /// 子类可额外注册自定义服务
        /// </summary>
        protected abstract void OnRegisterCustomServices(IContainerBuilder builder);

        protected virtual void Start()
        {
            OnStart();
        }

        /// <summary>
        /// 子类实现此方法指定启动 UI 界面等逻辑
        /// </summary>
        protected abstract void OnStart();
    }
}
