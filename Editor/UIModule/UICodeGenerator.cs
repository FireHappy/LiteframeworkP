using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using LiteFramework.Configs;
using LiteFramework.EditorTools;

namespace LiteFramework.EditorTools
{
    public static class UIPrefabCodeGenerator
    {
        [MenuItem("Assets/Generate UI MVP Template", true)]
        private static bool ValidateGenerate()
        {
            var obj = Selection.activeObject;
            return obj is GameObject && AssetDatabase.GetAssetPath(obj).EndsWith(".prefab");
        }

        [MenuItem("Assets/Generate UI MVP Template")]
        private static void GenerateUIMVP()
        {
            var go = Selection.activeObject as GameObject;
            string uiName = go.name.Replace("View", "");

            var config = LoadUIGeneratorConfig();
            if (config == null)
            {
                Debug.LogError("Can't Find UIGeneratorConfig");
                return;
            }

            GenerateCodeFiles(go, uiName, config);

            // 延迟生成路由，避免编译期间类型未加载
            EditorPrefs.SetBool("LiteFramework.PendingRouterGeneration", true);
            EditorPrefs.SetString("LiteFramework.RouterOutputPath", config.outputRootPath);

            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Generate success!", $"Generate {uiName} MVP code success.\nRouter will be generated after domain reload.", "Sure");
        }

        private static void GenerateCodeFiles(GameObject go, string uiName, UIGeneratorConfig config)
        {
            string viewTemplate = File.ReadAllText("Packages/com.liteframework.unity/Runtime/DefaultAssets/Templates/UIViewTemplate.txt");
            string viewAutoTemplate = File.ReadAllText("Packages/com.liteframework.unity/Runtime/DefaultAssets/Templates/UIViewAutoTemplate.txt");
            string presenterTemplate = File.ReadAllText("Packages/com.liteframework.unity/Runtime/DefaultAssets/Templates/UIPresenterTemplate.txt");

            var (fields, fieldsFind) = GenerateComponentFields(go.transform, config);
            var nameSpace = config.nameSpace;

            string viewCode = viewTemplate
                .Replace("{UI_NAME}", uiName)
                .Replace("{NAMESPACE}", nameSpace);

            string viewAutoCode = viewAutoTemplate
                .Replace("{UI_NAME}", uiName)
                .Replace("{Fields}", fields)
                .Replace("{FieldsFind}", fieldsFind)
                .Replace("{NAMESPACE}", nameSpace);

            string presenterCode = presenterTemplate
                .Replace("{UI_NAME}", uiName)
                .Replace("{NAMESPACE}", nameSpace);

            string outputDir = Path.Combine(config.outputRootPath, uiName);
            Directory.CreateDirectory(outputDir);

            File.WriteAllText(Path.Combine(outputDir, $"{uiName}View.Auto.cs"), viewAutoCode);

            var viewFile = Path.Combine(outputDir, $"{uiName}View.cs");
            if (!File.Exists(viewFile))
            {
                File.WriteAllText(viewFile, viewCode);
            }

            var presenterFile = Path.Combine(outputDir, $"{uiName}Presenter.cs");
            if (!File.Exists(presenterFile))
            {
                File.WriteAllText(presenterFile, presenterCode);
            }
        }

        private static (string, string) GenerateComponentFields(Transform root, UIGeneratorConfig config)
        {
            StringBuilder fields = new StringBuilder();
            StringBuilder fieldsFind = new StringBuilder();
            HashSet<string> usedNames = new HashSet<string>();

            Stack<Transform> stack = new Stack<Transform>();
            stack.Push(root);

            while (stack.Count > 0)
            {
                Transform current = stack.Pop();

                if (current != root)
                {
                    string name = current.name;
                    string[] parts = name.Split('_');

                    if (parts.Length >= 2)
                    {
                        string prefix = parts[0];
                        string fieldName = parts[1];

                        Type type = config.GetComponentTypeFromPrefix(prefix);
                        if (type != null)
                        {
                            var component = current.GetComponent(type);
                            if (component != null)
                            {
                                string varName = $"{prefix}_{fieldName}";

                                if (!usedNames.Contains(varName))
                                {
                                    usedNames.Add(varName);

                                    string camelCaseName = char.ToLower(prefix[0]) + prefix.Substring(1) + fieldName;

                                    fields.AppendLine($"\t\tpublic {type.Name} {camelCaseName};");
                                    fieldsFind.AppendLine($"\t\t\t{camelCaseName} = transform.Find(\"{GetFindPath(root, current)}\").GetComponent<{type.Name}>();");
                                }
                            }
                        }
                    }
                }

                for (int i = current.childCount - 1; i >= 0; i--)
                {
                    stack.Push(current.GetChild(i));
                }
            }

            return (fields.ToString(), fieldsFind.ToString());
        }

        private static string GetFindPath(Transform root, Transform child)
        {
            if (child == root) return "";

            List<string> path = new List<string>();
            Transform current = child;

            while (current != null && current != root)
            {
                path.Add(current.name);
                current = current.parent;
            }

            path.Reverse();
            return string.Join("/", path);
        }

        public static UIGeneratorConfig LoadUIGeneratorConfig()
        {
            string[] guids = AssetDatabase.FindAssets("t:UIGeneratorConfig");
            if (guids.Length == 0)
            {
                Debug.LogError("UIGeneratorConfig not found!");
                return null;
            }

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            var config = AssetDatabase.LoadAssetAtPath<UIGeneratorConfig>(path);

            if (config == null)
            {
                Debug.LogError("Failed to load UIGeneratorConfig");
            }

            return config;
        }
    }
}


[InitializeOnLoad]
public static class UIRouterGenerationTrigger
{
    static UIRouterGenerationTrigger()
    {
        if (EditorPrefs.GetBool("LiteFramework.PendingRouterGeneration", false))
        {
            EditorApplication.delayCall += GenerateRouterAfterDelay;
        }
    }

    private static void GenerateRouterAfterDelay()
    {
        EditorPrefs.DeleteKey("LiteFramework.PendingRouterGeneration");

        string outputPath = EditorPrefs.GetString("LiteFramework.RouterOutputPath", null);
        EditorPrefs.DeleteKey("LiteFramework.RouterOutputPath");
        var config = UIPrefabCodeGenerator.LoadUIGeneratorConfig();
        if (!string.IsNullOrEmpty(outputPath))
        {
            try
            {
                Debug.Log("🌀 正在延迟生成 UIRouter...");

                LiteFramework.EditorTools.UIRouterGeneratorEditor.GenerateRouterFiles(outputPath, config.nameSpace);
                AssetDatabase.Refresh();

                Debug.Log("✅ 成功生成 UIRouter 映射表！");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ 延迟生成 UIRouter 失败：{e}");
            }
        }
    }
}
