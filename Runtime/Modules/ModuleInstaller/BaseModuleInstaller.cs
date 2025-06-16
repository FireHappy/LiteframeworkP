using UnityEngine;
using VContainer;
namespace LiteFramework.Module
{
    public abstract class BaseModuleInstaller : ScriptableObject, IModuleInstaller
    {
        public abstract void Install(IContainerBuilder builder);
    }
}
