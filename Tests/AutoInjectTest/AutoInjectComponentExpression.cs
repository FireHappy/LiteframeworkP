using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using LiteFramework.Core.Utility;
using UnityEngine;

namespace LiteFramework.Tests
{

    public static class AutoInjectComponentExpression
    {
        // 缓存字段赋值的委托
        private static readonly Dictionary<FieldInfo, Action<object, object>> FieldSetterCache = new();

        public static void AutoInject(Transform root, object target)
        {
            if (root == null || target == null)
            {
                Debug.LogWarning("AutoInjectComponent: Root or target is null.");
                return;
            }

            var targetType = target.GetType();
            var allFields = targetType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var fieldsToInject = new Dictionary<FieldInfo, string>();

            foreach (var field in allFields)
            {
                var attribute = field.GetCustomAttribute<AutowritedAttribute>();
                if (attribute != null)
                {
                    var targetName = string.IsNullOrEmpty(attribute.targetObjName) ? field.Name : attribute.targetObjName;
                    fieldsToInject[field] = targetName.ToLower();
                }
            }

            var nodeMap = new Dictionary<string, Transform>(StringComparer.OrdinalIgnoreCase);
            FlattenHierarchy(root, nodeMap);

            foreach (var pair in fieldsToInject)
            {
                var field = pair.Key;
                var searchName = pair.Value;

                if (nodeMap.TryGetValue(searchName, out var node))
                {
                    var fieldType = field.FieldType;
                    object value = null;

                    if (fieldType == typeof(GameObject))
                    {
                        value = node.gameObject;
                    }
                    else
                    {
                        value = node.GetComponent(fieldType);
                        if (value == null)
                        {
                            Debug.LogWarning($"AutoInjectComponent: [{field.Name}] 找到了节点但没有对应组件：{fieldType.Name}");
                            continue;
                        }
                    }

                    // 表达式树赋值
                    GetFieldSetter(field)(target, value);
                }
                else
                {
                    Debug.LogWarning($"AutoInjectComponent: 未找到对应节点：{searchName}");
                }
            }
        }

        private static void FlattenHierarchy(Transform root, Dictionary<string, Transform> map)
        {
            Queue<Transform> queue = new();
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

        /// <summary>
        /// 创建并缓存字段的赋值表达式委托
        /// </summary>
        private static Action<object, object> GetFieldSetter(FieldInfo fieldInfo)
        {
            if (FieldSetterCache.TryGetValue(fieldInfo, out var setter))
                return setter;

            var targetExp = Expression.Parameter(typeof(object), "target");
            var valueExp = Expression.Parameter(typeof(object), "value");

            var castTarget = Expression.Convert(targetExp, fieldInfo.DeclaringType);
            var castValue = Expression.Convert(valueExp, fieldInfo.FieldType);

            var fieldExp = Expression.Field(castTarget, fieldInfo);
            var assignExp = Expression.Assign(fieldExp, castValue);

            var lambda = Expression.Lambda<Action<object, object>>(assignExp, targetExp, valueExp);
            setter = lambda.Compile();

            FieldSetterCache[fieldInfo] = setter;
            return setter;
        }
    }
}
