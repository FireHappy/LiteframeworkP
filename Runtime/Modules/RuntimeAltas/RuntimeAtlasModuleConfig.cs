using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace LiteFramework.Module
{

    [System.Serializable]
    public class RuntimeAtlasConfig
    {
        public int atlasSize = 2048;
        public int padding = 2;
        public PackingAlgorithm packingAlgorithm = PackingAlgorithm.SkyLine;
    }

    [System.Serializable]
    public class RuntimeAtlasModuleEntry
    {
        public string moduleKey;
        public RuntimeAtlasConfig config;
    }

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


