using UnityEditor;
using UnityEngine;
using LiteFramework.Configs;
using System.IO;

public static class UIConfigCreator
{
    private const string UICreatorConfigPath = "Packages/com.liteframework.unity/Runtime/DefaultAssets/Configs/UIGeneratorConfig.asset";

    [MenuItem("Assets/Create/LiteFramework/UI Generator Config")]
    public static void CreateConfigFromDefault()
    {
        var defaultConfig = AssetDatabase.LoadAssetAtPath<UIGeneratorConfig>(UICreatorConfigPath);
        if (defaultConfig == null)
        {
            Debug.LogError("默认配置未找到: " + UICreatorConfigPath);
            return;
        }

        var newConfig = Object.Instantiate(defaultConfig);

        string savePath = EditorUtility.SaveFilePanelInProject(
            "保存 UIGeneratorConfig",
            "UIGeneratorConfig",
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

    private const string UIConfigPath = "Packages/com.liteframework.unity/Runtime/DefaultAssets/Configs/UIConfig.asset";

    [MenuItem("Assets/Create/LiteFramework/UI Config")]
    public static void CreateUIRootConfig()
    {
        var defaultConfig = AssetDatabase.LoadAssetAtPath<UIConfig>(UIConfigPath);
        if (defaultConfig == null)
        {
            Debug.LogError("默认配置未找到: " + UIConfigPath);
            return;
        }

        var newConfig = Object.Instantiate(defaultConfig);

        string savePath = EditorUtility.SaveFilePanelInProject(
            "保存 UIConfig",
            "UIConfig",
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
