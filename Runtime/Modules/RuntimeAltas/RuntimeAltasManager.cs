using System.Collections.Generic;
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
            public Dictionary<string, AtlasResult> uniqueKeyCache = new();
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
            return AddTexture(moduleKey, texture, null);
        }

        public AtlasResult AddTexture(string moduleKey, Texture texture, string uniqueKey)
        {
            if (!moduleAtlasMap.TryGetValue(moduleKey, out var group))
            {
                Debug.LogError($"[RuntimeAtlasManager] Module '{moduleKey}' not registered.");
                return default;
            }

            // 优先检查路径缓存
            if (!string.IsNullOrEmpty(uniqueKey) && group.uniqueKeyCache.TryGetValue(uniqueKey, out var resultFromPath))
            {
                // Debug.Log("[RuntimeAtlasManager] Get Texture From uniqueKey cached");
                return resultFromPath;
            }

            // 检查 texture 实例缓存
            if (texture != null && group.textureCache.TryGetValue(texture, out var resultFromTex))
            {
                // Debug.Log("[RuntimeAtlasManager] Get Texture From texture cached");
                return resultFromTex;
            }

            // 图集中查找可用空间
            AtlasResult result;
            foreach (var atlas in group.atlases)
            {
                if (atlas.TryAddTexture(texture, out result))
                {
                    if (texture != null) group.textureCache[texture] = result;
                    if (!string.IsNullOrEmpty(uniqueKey)) group.uniqueKeyCache[uniqueKey] = result;
                    return result;
                }
            }

            // 创建新图集
            var newAtlas = new RuntimeAtlas(
                group.config.atlasSize,
                group.config.padding,
                group.config.packingAlgorithm,
                group.config.blitMaterial
            );

            result = newAtlas.AddTexture(texture);
            group.atlases.Add(newAtlas);

            if (!string.IsNullOrEmpty(uniqueKey))
            {
                group.uniqueKeyCache[uniqueKey] = result;
            }
            else if (texture != null)
            {
                group.textureCache[texture] = result;
            }
            return result;
        }

        public Texture GetAtlasTexture(string moduleKey, int index = 0)
        {
            if (moduleAtlasMap.TryGetValue(moduleKey, out var group))
            {
                if (index >= 0 && index < group.atlases.Count)
                {
                    return group.atlases[index].Texture;
                }
            }
            return null;
        }

        public void Clear(string moduleKey = null)
        {
            if (moduleKey != null)
            {
                if (moduleAtlasMap.TryGetValue(moduleKey, out var group))
                {
                    foreach (var atlas in group.atlases)
                    {
                        atlas.Dispose();
                    }
                    group.atlases.Clear();
                    group.textureCache.Clear();
                    group.uniqueKeyCache.Clear();
                }
            }
            else
            {
                foreach (var group in moduleAtlasMap.Values)
                {
                    foreach (var atlas in group.atlases)
                    {
                        atlas.Dispose();
                    }
                    group.atlases.Clear();
                    group.textureCache.Clear();
                    group.uniqueKeyCache.Clear();
                }
            }
        }
    }
}
