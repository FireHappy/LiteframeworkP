using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using LiteFramework.Configs;

namespace LiteFramework.EditorTools
{
    [CustomEditor(typeof(UIGeneratorConfig))]
    public class UIGeneratorConfigEditor : Editor
    {
        private SerializedProperty mappings;
        private SerializedProperty outputRootPathProp;
        private SerializedProperty nameSpaceProp;

        private List<Type> availableTypes;

        private void OnEnable()
        {
            mappings = serializedObject.FindProperty("mappings");
            outputRootPathProp = serializedObject.FindProperty("outputRootPath");
            nameSpaceProp = serializedObject.FindProperty("nameSpace");

            availableTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(asm => asm.GetTypes())
                .Where(t => typeof(Component).IsAssignableFrom(t) && !t.IsAbstract && t.IsPublic)
                .OrderBy(t => t.FullName)
                .ToList();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("MVP 路径配置", EditorStyles.boldLabel);
            DrawFolderPathField("输出路径", outputRootPathProp);
            DrawField("命名空间", nameSpaceProp);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("前缀组件映射", EditorStyles.boldLabel);

            for (int i = 0; i < mappings.arraySize; i++)
            {
                var element = mappings.GetArrayElementAtIndex(i);
                var prefixProp = element.FindPropertyRelative("prefix");
                var typeNameProp = element.FindPropertyRelative("componentType").FindPropertyRelative("typeName");

                EditorGUILayout.BeginHorizontal();

                // 输入前缀
                prefixProp.stringValue = EditorGUILayout.TextField(prefixProp.stringValue, GUILayout.Width(100));

                // 当前类型显示简写名
                string currentTypeShort = "(未设置)";
                var currentType = Type.GetType(typeNameProp.stringValue);
                if (currentType != null)
                    currentTypeShort = currentType.Name;

                // 获取当前按钮位置
                Rect popupRect = GUILayoutUtility.GetRect(new GUIContent(currentTypeShort), EditorStyles.popup);

                if (GUI.Button(popupRect, currentTypeShort, EditorStyles.popup))
                {
                    // 弹出类型选择器窗口
                    SearchableTypePopupLauncher.Show(popupRect, availableTypes, typeNameProp.stringValue, selected =>
                    {
                        typeNameProp.stringValue = selected;
                        serializedObject.ApplyModifiedProperties();
                    });
                }

                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    mappings.DeleteArrayElementAtIndex(i);
                    break;
                }

                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("+ 添加新映射"))
            {
                mappings.arraySize++;
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawFolderPathField(string label, SerializedProperty pathProp)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);
            pathProp.stringValue = EditorGUILayout.TextField(pathProp.stringValue);
            if (GUILayout.Button("浏览", GUILayout.Width(60)))
            {
                string defaultPath = string.IsNullOrEmpty(pathProp.stringValue) ? Application.dataPath :
                    System.IO.Path.Combine(Application.dataPath, pathProp.stringValue.TrimStart("Assets/".ToCharArray()));

                string folder = EditorUtility.OpenFolderPanel($"选择{label}", defaultPath, "");
                if (!string.IsNullOrEmpty(folder) && folder.StartsWith(Application.dataPath))
                {
                    folder = "Assets" + folder.Substring(Application.dataPath.Length);
                    pathProp.stringValue = folder;
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawField(string label, SerializedProperty prop)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);
            prop.stringValue = EditorGUILayout.TextField(prop.stringValue);
            EditorGUILayout.EndHorizontal();
        }
    }
}
