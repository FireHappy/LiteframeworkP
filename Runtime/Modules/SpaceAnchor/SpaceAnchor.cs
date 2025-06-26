using UnityEngine;

namespace LiteFramework.Module
{

    public class SpaceAnchor
    {
        public string AnchorId { get; }
        public SpaceAnchorInfo Info { get; }
        public GameObject Root { get; }

        public Transform UIContainer => string.IsNullOrEmpty(Info?.UIContainerPath)
            ? Root.transform
            : Root.transform.Find(Info.UIContainerPath);

        public SpaceAnchor(string anchorId, SpaceAnchorInfo info, GameObject root)
        {
            AnchorId = anchorId;
            Info = info;
            Root = root;
        }
    }
}