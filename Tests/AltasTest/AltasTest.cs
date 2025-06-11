using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using LiteFramework.Module;
using Debug = UnityEngine.Debug;

namespace LiteFramework.Tests
{
    public class AtlasTest : MonoBehaviour
    {
        public RuntimeAtlasModuleConfig config;
        public RectTransform container;
        public GameObject rawImagePrefab; // 包含 RawImage 组件的预设
        public List<Texture2D> testTextures; // 拖入测试图

        private RuntimeAtlasManager atlasManager;
        private Stopwatch stopwatch = new Stopwatch();
        private List<float> atlasLoadTimes = new List<float>();
        private List<float> originalLoadTimes = new List<float>();

        void Start()
        {

            if (testTextures == null || testTextures.Count == 0)
            {
                Debug.LogError("请在Inspector中添加测试纹理");
                return;
            }


            if (container == null)
            {
                Debug.LogError("请在Inspector中指定UI容器");
                return;
            }

            if (rawImagePrefab == null)
            {
                Debug.LogError("请在Inspector中指定RawImage预设");
                return;
            }

            // 初始化图集管理器
            atlasManager = new RuntimeAtlasManager(config);

            // 显示测试开始信息
            Debug.Log($"===== 开始图集性能测试 =====");

            //图集冷加载
            RunSingleTest(true);
            RunSingleTest(false);
            // 图集热加载
            RunSingleTest(true);
        }

        private void RunSingleTest(bool useAtlas)
        {
            string testType = useAtlas ? "图集" : "原始纹理";


            // 强制垃圾回收
            System.GC.Collect();

            // 测量加载时间
            stopwatch.Reset();
            stopwatch.Start();

            if (useAtlas)
            {
                // 图集加载测试
                try
                {
                    for (int i = 0; i < testTextures.Count; i++)
                    {
                        AtlasResult result = atlasManager.AddTexture("Shop", testTextures[i]);
                        CreateUI(result, i, "Atlas_" + i);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"图集加载失败: {e.Message}");
                }
            }
            else
            {
                for (int i = 0; i < testTextures.Count; i++)
                {
                    CreateUI(testTextures[i], i, "Original_" + testTextures);
                }
            }

            // 等待渲染完成
            stopwatch.Stop();

            // 记录结果
            if (useAtlas)
            {
                atlasLoadTimes.Add(stopwatch.ElapsedMilliseconds);
            }
            else
            {
                originalLoadTimes.Add(stopwatch.ElapsedMilliseconds);
            }

            Debug.Log($" {testType}加载耗时: {stopwatch.ElapsedMilliseconds} ms");
        }

        private void CreateUI(AtlasResult atlasResult, int index, string name)
        {
            var go = GameObject.Instantiate(rawImagePrefab, container);
            go.name = name;

            var rawImage = go.GetComponent<RawImage>();
            if (rawImage == null)
            {
                Debug.LogError("RawImage组件未找到！");
                return;
            }

            if (name.Contains("Atlas"))
            {
                // 图集纹理显示
                if (atlasResult.texture == null)
                {
                    Debug.LogError("Atlas纹理为空！");
                    return;
                }

                rawImage.texture = atlasResult.texture;
                rawImage.uvRect = atlasResult.uv;
            }
            else
            {
                // 原始纹理显示
                rawImage.texture = testTextures[index];
                rawImage.uvRect = new Rect(0, 0, 1, 1);
            }
        }

        private void CreateUI(Texture2D texture, int index, string name)
        {
            var go = GameObject.Instantiate(rawImagePrefab, container);
            go.name = name;

            var rawImage = go.GetComponent<RawImage>();
            if (rawImage == null)
            {
                Debug.LogError("RawImage组件未找到！");
                return;
            }

            rawImage.texture = texture;
            rawImage.uvRect = new Rect(0, 0, 1, 1);
        }
    }
}