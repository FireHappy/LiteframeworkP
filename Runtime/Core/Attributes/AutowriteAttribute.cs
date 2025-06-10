using System;
using UnityEngine;

namespace LiteFramework.Core.Utility
{
    /// <summary>
    /// 自动注入特性,name 为空则使用字段名匹配,需要配合AutoInjectComponent
    /// Tip1: 字符串匹配不区分大小写
    /// Tip2: 字段为空的情况下才会激活自动注入的匹配
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class AutowritedAttribute : PropertyAttribute
    {
        public readonly string targetObjName;
        public AutowritedAttribute(string targetObjName)
        {
            this.targetObjName = targetObjName;
        }
        public AutowritedAttribute()
        {

        }
    }
}


