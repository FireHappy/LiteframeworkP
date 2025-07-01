using LiteFramework.Module.UI;
using UnityEngine;
using VContainer;

namespace LiteFramework.Module
{
    [CreateAssetMenu(menuName = "LiteFramework/Module/RuntimeAltasInstaller")]
    public class RuntimeAltasInstaller : BaseModuleInstaller
    {
        [SerializeField] private RuntimeAtlasModuleConfig config;

        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(config);
            builder.Register<RuntimeAtlasManager>(Lifetime.Singleton);
        }
    }
}

