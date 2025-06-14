using System.Collections.Generic;
using UnityEngine;



namespace LiteFramework.Module
{
    [System.Serializable]
    public class SpaceAnchorInfo
    {
        public string AnchorId;
        public string AnchorName;
        public Vector3 DefaultPosition;
        public Vector3 DefaultEulerAngle;
        public GameObject Prefab;
        public string UIContainerPath;
    }


    [CreateAssetMenu(fileName = "SpaceAnchorConfig", menuName = "LiteFramework/SpaceAnchor Config")]
    public class SpaceAnchorConfig : ScriptableObject
    {
        public List<SpaceAnchorInfo> Anchors = new List<SpaceAnchorInfo>();


        /// <summary>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public SpaceAnchorInfo GetAnchorInfo(string id)
        {
            return Anchors.Find(a => a.AnchorId == id);
        }

        public bool ContainsAnchor(string id)
        {
            return Anchors.Exists(a => a.AnchorId == id);
        }
    }

}