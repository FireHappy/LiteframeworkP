using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace LiteFramework.Module
{
    public class RuntimeAtlasManager
    {
        private class AtlasGroup
        {
            public RuntimeAtlasConfig config;
            public List<RuntimeAtlas> atlases = new();
            public Dictionary<Texture, AtlasResult> textureCache = new();
        }

        private readonly Dictionary<string, AtlasGroup> moduleAtlasMap = new();

        public RuntimeAtlasManager(RuntimeAtlasModuleConfig moduleConfig)
        {
            foreach (var entry in moduleConfig.modules)
            {
                if (entry != null && !string.IsNullOrEmpty(entry.moduleKey) && entry.config != null)
                {
                    RegisterModule(entry.moduleKey, entry.config);
                }
            }
        }

        public void RegisterModule(string key, RuntimeAtlasConfig config)
        {
            if (!moduleAtlasMap.ContainsKey(key))
            {
                moduleAtlasMap[key] = new AtlasGroup { config = config };
            }
            else
            {
                Debug.LogWarning($"[RuntimeAtlasManager] Duplicate module key: {key}");
            }
        }

        public AtlasResult AddTexture(string moduleKey, Texture texture)
        {
            if (!moduleAtlasMap.TryGetValue(moduleKey, out var group))
            {
                Debug.LogError($"[RuntimeAtlasManager] Module '{moduleKey}' not registered.");
                return default;
            }

            if (group.textureCache.TryGetValue(texture, out var cached))
                return cached;

            AtlasResult result;
            foreach (var item in group.atlases)
            {
                if (item.TryAddTexture(texture, out result))
                {
                    group.textureCache[texture] = result;
                    return result;
                }
            }
            //如果图集空间了扩容一个新的图集处理
            var altas = new RuntimeAtlas(group.config.atlasSize, group.config.padding, group.config.packingAlgorithm, group.config.blitMaterial);
            result = altas.AddTexture(texture);
            group.atlases.Add(altas);
            group.textureCache[texture] = result;
            return result;
        }

        public Texture GetAtlasTexture(string moduleKey, int index = 0)
        {
            if (moduleAtlasMap.TryGetValue(moduleKey, out var group))
            {
                if (index >= 0 && index < group.atlases.Count)
                    return group.atlases[index].Texture;
            }
            return null;
        }

        public void Clear(string moduleKey = null)
        {
            if (moduleKey != null)
            {
                if (moduleAtlasMap.TryGetValue(moduleKey, out var group))
                {
                    foreach (var item in group.atlases)
                    {
                        item.Dispose();
                    }
                    group.atlases.Clear();
                    group.textureCache.Clear();
                }
            }
            else
            {
                foreach (var group in moduleAtlasMap.Values)
                {
                    foreach (var item in group.atlases)
                    {
                        item.Dispose();
                    }
                    group.atlases.Clear();
                    group.textureCache.Clear();
                }
            }
        }
    }
}
