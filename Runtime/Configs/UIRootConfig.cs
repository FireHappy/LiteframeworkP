using UnityEngine;

namespace LiteFramework.Configs
{
    public class UIConfig : ScriptableObject
    {
        /// <summary>
        /// UI加载的路径，相对Resource
        /// </summary>
        public string UIPath = "UI";
        /// <summary>
        /// 标记默认UIParent的Tag
        /// </summary>
        public string DefaultUIParentTag = "UIParent";
        /// <summary>
        /// 标记默认DialogParent的Tag
        /// </summary>
        public string DefaultUIDialogTag = "DialogParent";
        /// <summary>
        /// UI被回收到UI池中保活时间单位（秒）
        /// </summary>
        public float UIKeepAliveTime = 60;
        /// <summary>
        /// UI的根预制体，如果场景中没有，会实例化该预制体创建
        /// </summary>
        public GameObject RootUIPrefab;
    }
}
