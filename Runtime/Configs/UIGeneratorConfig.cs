using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LiteFramework.Configs
{
    [Serializable]
    public class ComponentTypeRef
    {
        [SerializeField] private string typeName;

        public Type Type => string.IsNullOrEmpty(typeName) ? null : Type.GetType(typeName);

        public void SetType(Type type)
        {
            if (type == null || !typeof(Component).IsAssignableFrom(type))
            {
                Debug.LogError("必须是 UnityEngine.Component 的子类");
                return;
            }

            typeName = type.AssemblyQualifiedName;
        }

        public string TypeDisplayName => Type != null ? Type.Name : "(未设置)";
        public string TypeFullName => typeName;
    }

    [Serializable]
    public class PrefixComponentMapping
    {
        public string prefix;
        public ComponentTypeRef componentType;
    }

    public class UIGeneratorConfig : ScriptableObject
    {
        public string outputRootPath = "Assets/Scripts/Features";
        public string nameSpace = "LiteFramework.Sample";

        public List<PrefixComponentMapping> mappings = new List<PrefixComponentMapping>();

        public Type GetComponentTypeFromPrefix(string prefix)
        {
            foreach (var map in mappings)
            {
                if (string.Equals(map.prefix, prefix, StringComparison.OrdinalIgnoreCase))
                {
                    return map.componentType?.Type;
                }
            }

            return null;
        }
    }
}
