using UnityEngine;
using VContainer;
namespace LiteFramework.Module
{
    public interface IModuleInstaller
    {
        void Install(IContainerBuilder builder);
    }
}

