using LiteFramework.Module.UI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace LiteFramework.Module
{
    [CreateAssetMenu(menuName = "LiteFramework/Module/UIModuleInstaller")]
    public class UIModuleInstaller : BaseModuleInstaller
    {
        [SerializeField] private UIConfig config;

        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(config);
            builder.RegisterEntryPoint<UIPoolManager>(Lifetime.Singleton).AsSelf();
            builder.Register<IUIManager, UIManager>(Lifetime.Singleton);
            builder.Register<UIRouter>(Lifetime.Singleton);
        }
    }
}

