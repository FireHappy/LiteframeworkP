
namespace LiteFramework.Module.Editor
{
#if UNITY_EDITOR
    using UnityEditor;
    using UnityEngine;
    using System.Collections.Generic;
    using LiteFramework.Module;

    [CustomEditor(typeof(SpaceAnchorConfig))]
    public class SpaceAnchorConfigDrawer : Editor
    {
        private SerializedProperty anchorsProp;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            anchorsProp = serializedObject.FindProperty("Anchors");

            CheckDuplicates(out var duplicatePrefabs, out var duplicateIds);

            // ===== 显示重复信息 =====
            if (duplicatePrefabs.Count > 0 || duplicateIds.Count > 0)
            {
                EditorGUILayout.HelpBox("发现重复项，请检查以下问题！", MessageType.Error);

                if (duplicateIds.Count > 0)
                {
                    EditorGUILayout.LabelField("⚠ 重复的 AnchorId：", EditorStyles.boldLabel);
                    foreach (var id in duplicateIds)
                    {
                        EditorGUILayout.LabelField($"- {id}");
                    }
                }

                if (duplicatePrefabs.Count > 0)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("⚠ 重复的 Prefab：", EditorStyles.boldLabel);
                    foreach (var prefab in duplicatePrefabs)
                    {
                        EditorGUILayout.ObjectField(prefab, typeof(GameObject), false);
                    }
                }

                EditorGUILayout.Space();
            }

            // ===== 绘制 Anchors 列表（调用自定义 Drawer） =====
            EditorGUILayout.LabelField("锚点配置列表", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            for (int i = 0; i < anchorsProp.arraySize; i++)
            {
                SerializedProperty elementProp = anchorsProp.GetArrayElementAtIndex(i);

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.BeginHorizontal();

                // 折叠框 + 调用 SpaceAnchorInfoDrawer
                elementProp.isExpanded = EditorGUILayout.Foldout(elementProp.isExpanded, $"锚点 [{i}]", true);
                if (GUILayout.Button("删除", GUILayout.Width(60)))
                {
                    anchorsProp.DeleteArrayElementAtIndex(i);
                    break; // 避免绘制已被删除元素
                }

                EditorGUILayout.EndHorizontal();

                if (elementProp.isExpanded)
                {
                    EditorGUILayout.PropertyField(elementProp, true); // 递归绘制，自定义 drawer 会生效
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }
            EditorGUI.indentLevel--;

            if (GUILayout.Button("添加锚点"))
            {
                anchorsProp.arraySize++;
                var newElement = anchorsProp.GetArrayElementAtIndex(anchorsProp.arraySize - 1);

                // 自动分配不重复的 AnchorId
                HashSet<string> existingIds = new HashSet<string>();
                for (int i = 0; i < anchorsProp.arraySize - 1; i++)
                {
                    var id = anchorsProp.GetArrayElementAtIndex(i).FindPropertyRelative("AnchorId").stringValue;
                    existingIds.Add(id);
                }

                int idIndex = 0;
                string newId = $"Anchor_{idIndex}";
                while (existingIds.Contains(newId))
                {
                    idIndex++;
                    newId = $"Anchor_{idIndex}";
                }

                newElement.FindPropertyRelative("AnchorId").stringValue = newId;
                newElement.FindPropertyRelative("AnchorName").stringValue = ""; // 会在绘制时从 Prefab 名字自动填充
                newElement.FindPropertyRelative("Prefab").objectReferenceValue = null;
                newElement.FindPropertyRelative("UIContainerPath").stringValue = "";
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void CheckDuplicates(out List<GameObject> duplicatePrefabs, out List<string> duplicateIds)
        {
            duplicatePrefabs = new List<GameObject>();
            duplicateIds = new List<string>();

            Dictionary<GameObject, int> prefabCount = new Dictionary<GameObject, int>();
            Dictionary<string, int> idCount = new Dictionary<string, int>();

            for (int i = 0; i < anchorsProp.arraySize; i++)
            {
                var anchorProp = anchorsProp.GetArrayElementAtIndex(i);
                var prefabProp = anchorProp.FindPropertyRelative("Prefab");
                var idProp = anchorProp.FindPropertyRelative("AnchorId");

                var prefab = prefabProp?.objectReferenceValue as GameObject;
                string id = idProp?.stringValue;

                if (prefab != null)
                {
                    if (!prefabCount.ContainsKey(prefab))
                        prefabCount[prefab] = 0;
                    prefabCount[prefab]++;
                }

                if (!string.IsNullOrEmpty(id))
                {
                    if (!idCount.ContainsKey(id))
                        idCount[id] = 0;
                    idCount[id]++;
                }
            }

            foreach (var kvp in prefabCount)
            {
                if (kvp.Value > 1)
                    duplicatePrefabs.Add(kvp.Key);
            }

            foreach (var kvp in idCount)
            {
                if (kvp.Value > 1)
                    duplicateIds.Add(kvp.Key);
            }
        }
    }
#endif

}