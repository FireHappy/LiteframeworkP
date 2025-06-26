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
        }

        protected override void OnSetContainer(IObjectResolver container)
        {
            base.OnSetContainer(container);
            GlobalContainer.SetContainer(container);
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
        /// 自动注册带注册
        /// </summary>
        private void RegisterAllAutoRegister(IContainerBuilder builder)
        {
            VContainerAutoRegister.Register(builder);
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
