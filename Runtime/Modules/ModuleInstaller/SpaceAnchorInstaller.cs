using LiteFramework.Module.UI;
using UnityEngine;
using VContainer;

namespace LiteFramework.Module
{
    [CreateAssetMenu(menuName = "LiteFramework/Module/SpaceAnchorInstaller")]
    public class SpaceAnchorInstaller : BaseModuleInstaller
    {
        [SerializeField] private SpaceAnchorConfig config;

        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(config);
            builder.Register<SpaceAnchorManager>(Lifetime.Singleton);
        }
    }
}

