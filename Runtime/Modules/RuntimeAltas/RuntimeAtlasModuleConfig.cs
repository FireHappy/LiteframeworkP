using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace LiteFramework.Module
{

    [System.Serializable]
    public class RuntimeAtlasConfig
    {
        public Material blitMaterial;
        public int atlasSize = 2048;
        public int padding = 2;
    }

    [System.Serializable]
    public class RuntimeAtlasModuleEntry
    {
        public string moduleKey;
        public RuntimeAtlasConfig config;
    }


    [CreateAssetMenu(fileName = "RuntimeAtlasConfig", menuName = "LiteFramework/RuntimeAltasConfig")]
    public class RuntimeAtlasModuleConfig : ScriptableObject
    {
        public List<RuntimeAtlasModuleEntry> modules = new();

        public RuntimeAtlasConfig GetConfig(string moduleKey)
        {
            foreach (var entry in modules)
            {
                if (entry.moduleKey == moduleKey)
                    return entry.config;
            }

            Debug.LogWarning($"[RuntimeAtlasModuleConfig] No config found for key: {moduleKey}");
            return null;
        }
    }

}


