using UnityEngine;
using VContainer;
using VContainer.Unity;
using System.Linq;
using System.Reflection;
using LiteFramework.Core.Utility;
using LiteFramework.Module;

namespace LiteFramework
{
    /// <summary>
    /// 纯净版程序启动基类，只处理模块注册和全局容器
    /// </summary>
    public abstract class LiteStartupBase : LifetimeScope
    {
        [SerializeField]
        protected BaseModuleInstaller[] ModuleInstallers;

        protected override void Configure(IContainerBuilder builder)
        {
            RegisterAllConfiguredModules(builder);
            RegisterAllAutoRegister(builder);
            OnRegisterCustomServices(builder);
            GlobalContainer.SetContainer(Container);
        }

        /// <summary>
        /// 注册所有 ScriptableObject 实现的模块安装器
        /// </summary>
        private void RegisterAllConfiguredModules(IContainerBuilder builder)
        {
            if (ModuleInstallers == null) return;

            foreach (var installerSO in ModuleInstallers)
            {
                if (installerSO is IModuleInstaller installer)
                {
                    installer.Install(builder);
                }
                else
                {
                    Debug.LogError($"模块Installer配置 {installerSO.name} 未实现 IModuleInstaller 接口");
                }
            }
        }

        /// <summary>
        /// 自动注册带特性类（可选）
        /// </summary>
        private void RegisterAllAutoRegister(IContainerBuilder builder)
        {
            var assemblies = GetAutoRegisterAssemblies();
            VContainerAutoRegister.RegisterWithAttribute(builder, assemblies);
        }

        /// <summary>
        /// 默认注册程序集（可重写）
        /// </summary>
        protected virtual Assembly[] GetAutoRegisterAssemblies()
        {
            return new[] { Assembly.Load("Assembly-CSharp") };
        }

        /// <summary>
        /// 可选自定义服务注册
        /// </summary>
        protected virtual void OnRegisterCustomServices(IContainerBuilder builder) { }

        protected virtual void Start()
        {
            OnStart();
        }

        protected abstract void OnStart();
    }
}
