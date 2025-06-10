using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace LiteFramework.Core.Utility
{
    /// <summary>
    /// 自动注入组件优化版本
    /// </summary>
    public static class AutoInjectComponent
    {
        /// <summary>
        /// 自动注入组件（优化版）
        /// </summary>
        /// <param name="root">UI 根节点</param>
        /// <param name="target">目标对象</param>
        public static void AutoInject(Transform root, object target)
        {
            if (root == null || target == null)
            {
                Debug.LogWarning("AutoInjectComponent: Root or target is null.");
                return;
            }

            var targetType = target.GetType();

            // 1. 获取所有带 Autowrited 特性的字段
            var allFields = targetType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var fieldsToInject = new Dictionary<FieldInfo, string>();

            foreach (var field in allFields)
            {
                var attribute = field.GetCustomAttribute<AutowritedAttribute>();
                if (attribute != null)
                {
                    var targetName = string.IsNullOrEmpty(attribute.targetObjName) ? field.Name : attribute.targetObjName;
                    fieldsToInject[field] = targetName.ToLower(); // 忽略大小写
                }
            }

            // 2. 扁平化所有子节点，构建字典：name → transform
            var nodeMap = new Dictionary<string, Transform>(StringComparer.OrdinalIgnoreCase);
            FlattenHierarchy(root, nodeMap);

            // 3. 匹配字段并赋值
            foreach (var pair in fieldsToInject)
            {
                var field = pair.Key;
                var searchName = pair.Value;

                if (nodeMap.TryGetValue(searchName, out var node))
                {
                    var fieldType = field.FieldType;

                    if (fieldType == typeof(GameObject))
                    {
                        field.SetValue(target, node.gameObject);
                    }
                    else
                    {
                        var component = node.GetComponent(fieldType);
                        if (component != null)
                        {
                            field.SetValue(target, component);
                        }
                        else
                        {
                            Debug.LogWarning($"AutoInjectComponent: [{field.Name}] 找到了节点但没有对应组件：{fieldType.Name}");
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"AutoInjectComponent: 未找到对应节点：{searchName}");
                }
            }
        }

        /// <summary>
        /// 扁平化所有子节点
        /// </summary>
        private static void FlattenHierarchy(Transform root, Dictionary<string, Transform> map)
        {
            Queue<Transform> queue = new Queue<Transform>();
            queue.Enqueue(root);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                var key = current.name.ToLower();
                if (!map.ContainsKey(key))
                {
                    map.Add(key, current);
                }

                foreach (Transform child in current)
                {
                    queue.Enqueue(child);
                }
            }
        }
    }
}
