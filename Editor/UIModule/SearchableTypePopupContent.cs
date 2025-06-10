using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LiteFramework.EditorTools
{
    public class SearchableTypePopupContent : PopupWindowContent
    {
        private Action<string> onSelected;
        private List<string> displayNames;
        private List<string> fullTypeNames;
        private string searchText = "";
        private Vector2 scroll;

        private GUIStyle buttonStyle;
        private static GUIStyle SafeSearchFieldStyle => GUI.skin.FindStyle("ToolbarSearchTextField") ?? EditorStyles.textField;

        public SearchableTypePopupContent(List<Type> types, string currentType, Action<string> onSelected)
        {
            this.onSelected = onSelected;
            displayNames = new List<string>();
            fullTypeNames = new List<string>();

            foreach (var type in types)
            {
                displayNames.Add(type.FullName);
                fullTypeNames.Add(type.AssemblyQualifiedName);
            }
        }

        public override Vector2 GetWindowSize()
        {
            float height = Mathf.Min(300, displayNames.Count * (EditorGUIUtility.singleLineHeight + 6) + 40);
            return new Vector2(300, height);
        }

        public override void OnOpen()
        {
            buttonStyle = new GUIStyle(EditorStyles.label)
            {
                margin = new RectOffset(0, 0, 0, 0),
                padding = new RectOffset(4, 4, 2, 2),
                fixedHeight = EditorGUIUtility.singleLineHeight + 4
            };
        }

        public override void OnGUI(Rect rect)
        {
            // 搜索框
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            searchText = EditorGUILayout.TextField(searchText, SafeSearchFieldStyle, GUILayout.ExpandWidth(true));

            var cancelStyle = GUI.skin.FindStyle("ToolbarSearchCancelButton");
            if (GUILayout.Button("", cancelStyle ?? EditorStyles.miniButton, GUILayout.Width(20)))
            {
                searchText = "";
                GUI.FocusControl(null);
                GUI.changed = true;
            }

            EditorGUILayout.EndHorizontal();

            // 列表内容
            scroll = EditorGUILayout.BeginScrollView(scroll);
            GUILayout.Space(2);

            for (int i = 0; i < displayNames.Count; i++)
            {
                if (!string.IsNullOrEmpty(searchText) && !displayNames[i].ToLower().Contains(searchText.ToLower()))
                    continue;

                if (GUILayout.Button(displayNames[i], buttonStyle, GUILayout.Height(buttonStyle.fixedHeight)))
                {
                    onSelected?.Invoke(fullTypeNames[i]);
                    editorWindow.Close();
                }
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndScrollView();
        }
    }

    public static class SearchableTypePopupLauncher
    {
        public static void Show(Rect activatorRect, List<Type> types, string currentType, Action<string> onSelected)
        {
            PopupWindow.Show(activatorRect, new SearchableTypePopupContent(types, currentType, onSelected));
        }
    }
}
