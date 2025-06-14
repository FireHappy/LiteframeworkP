using UnityEditor;
using UnityEngine;
using LiteFramework.Module;
using System.IO;

public static class RuntimeAtlasConfigCreator
{
    // 默认配置路径（你可以将默认的 RuntimeAtlasModuleConfig.asset 放到这个路径下）
    private const string DefaultAtlasConfigPath = "Packages/com.liteframework.unity/Runtime/DefaultAssets/Configs/RuntimeAtlasConfig.asset";

    [MenuItem("Assets/Create/LiteFramework/Runtime Atlas Config")]
    public static void CreateAtlasConfigFromDefault()
    {
        var defaultConfig = AssetDatabase.LoadAssetAtPath<RuntimeAtlasModuleConfig>(DefaultAtlasConfigPath);
        if (defaultConfig == null)
        {
            Debug.LogError("默认配置未找到: " + DefaultAtlasConfigPath);
            return;
        }

        var newConfig = Object.Instantiate(defaultConfig);

        string savePath = EditorUtility.SaveFilePanelInProject(
            "保存 RuntimeAtlasConfig",
            "RuntimeAtlasConfig",
            "asset",
            "选择保存路径");

        if (string.IsNullOrEmpty(savePath))
            return;

        AssetDatabase.CreateAsset(newConfig, savePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = newConfig;
    }
}
