
namespace LiteFramework.Module.Editor
{
#if UNITY_EDITOR
    using UnityEditor;
    using UnityEngine;
    using System.Collections.Generic;
    using LiteFramework.Module;

    [CustomPropertyDrawer(typeof(SpaceAnchorInfo))]
    public class SpaceAnchorInfoDrawer : PropertyDrawer
    {
        private Dictionary<Transform, bool> foldoutStates = new Dictionary<Transform, bool>();
        private List<Transform> childTransforms = new List<Transform>();
        private Transform selectedTransform = null;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var prefabProp = property.FindPropertyRelative("Prefab");
            var pathProp = property.FindPropertyRelative("UIContainerPath");
            var anchorIdProp = property.FindPropertyRelative("AnchorId");
            var anchorNameProp = property.FindPropertyRelative("AnchorName");
            var defaultPosProp = property.FindPropertyRelative("DefaultPosition");
            var defaultEulerProp = property.FindPropertyRelative("DefaultEulerAngle");

            float lineHeight = EditorGUIUtility.singleLineHeight + 2;
            var rect = new Rect(position.x, position.y, position.width, lineHeight);

            EditorGUI.PropertyField(rect, anchorIdProp);
            rect.y += lineHeight;
            EditorGUI.PropertyField(rect, anchorNameProp);
            rect.y += lineHeight;
            EditorGUI.PropertyField(rect, prefabProp);
            rect.y += lineHeight;
            EditorGUI.PropertyField(rect, defaultPosProp);
            rect.y += lineHeight;
            EditorGUI.PropertyField(rect, defaultEulerProp);
            rect.y += lineHeight;

            GameObject prefab = prefabProp.objectReferenceValue as GameObject;
            if (prefab != null && string.IsNullOrEmpty(anchorNameProp.stringValue))
            {
                anchorNameProp.stringValue = prefab.name;
            }
            if (prefab != null)
            {
                if (childTransforms.Count == 0)
                {
                    LoadChildTransforms(prefab.transform);
                }

                // 绘制树形结构，从 prefab.transform 根开始
                EditorGUI.LabelField(rect, "Select UIContainer:");
                rect.y += lineHeight;

                rect.height = lineHeight;
                DrawTransformTree(prefab.transform, prefab.transform, ref rect, pathProp, prefab.transform);

                // 显示当前选中路径
                rect.y += 2;
                EditorGUI.LabelField(rect, "Selected Path:", pathProp.stringValue);
                rect.y += lineHeight;
            }
            else
            {
                childTransforms.Clear();
                selectedTransform = null;
                foldoutStates.Clear();
            }
        }


        private void DrawTransformTree(Transform current, Transform root, ref Rect rect, SerializedProperty pathProp, Transform prefabRoot, int indentLevel = 0)
        {
            float lineHeight = EditorGUIUtility.singleLineHeight + 2;

            if (!foldoutStates.ContainsKey(current))
                foldoutStates[current] = true;

            bool hasChildren = current.childCount > 0;

            Rect foldoutRect = new Rect(rect.x + indentLevel * 15, rect.y, 15, EditorGUIUtility.singleLineHeight);
            Rect labelRect = new Rect(foldoutRect.xMax, rect.y, rect.width - indentLevel * 15 - 15, EditorGUIUtility.singleLineHeight);

            // 绘制折叠箭头（只响应点击切换展开状态）
            if (hasChildren)
            {
                if (GUI.Button(foldoutRect, foldoutStates[current] ? "▼" : "▶", EditorStyles.label))
                {
                    foldoutStates[current] = !foldoutStates[current];
                }
            }

            // 高亮选中项背景
            if (selectedTransform == current)
            {
                EditorGUI.DrawRect(new Rect(labelRect.x, labelRect.y, labelRect.width, labelRect.height), new Color(0.24f, 0.48f, 0.90f, 0.3f));
            }

            // 显示名称（整行区域响应点击选中）
            EditorGUI.LabelField(labelRect, current.name);

            if (Event.current.type == EventType.MouseDown && labelRect.Contains(Event.current.mousePosition))
            {
                selectedTransform = current;
                pathProp.stringValue = GetRelativePath(current, prefabRoot);
                Event.current.Use();
            }

            rect.y += lineHeight;

            if (hasChildren && foldoutStates[current])
            {
                foreach (Transform child in current)
                {
                    DrawTransformTree(child, root, ref rect, pathProp, prefabRoot, indentLevel + 1);
                }
            }
        }


        private void LoadChildTransforms(Transform root)
        {
            childTransforms.Clear();
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                if (t != root) childTransforms.Add(t);
            }
        }

        private string GetRelativePath(Transform child, Transform root)
        {
            if (child == root) return "";
            string path = child.name;
            while (child.parent != null && child.parent != root)
            {
                child = child.parent;
                path = child.name + "/" + path;
            }
            return path;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // 预留足够高度给树形结构绘制（可根据需求调整）
            return (EditorGUIUtility.singleLineHeight + 2) * 12;
        }
    }
#endif

}