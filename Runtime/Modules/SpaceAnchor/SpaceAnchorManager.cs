using System.Collections.Generic;
using LiteFramework.Core.Utility;
using UnityEngine;

namespace LiteFramework.Module
{
    public class SpaceAnchorManager
    {
        private Dictionary<string, GameObject> anchorCache = new Dictionary<string, GameObject>();
        private SpaceAnchorConfig config;

        public SpaceAnchorManager(SpaceAnchorConfig config)
        {
            this.config = config;
        }

        public (SpaceAnchorInfo, GameObject) CreateAnchor(string anchorId, Transform parent = null, Pose? pose = null)
        {
            var anchor = config.GetAnchorInfo(anchorId);
            if (!anchorCache.TryGetValue(anchorId, out GameObject anchorObj))
            {
                if (anchor != null)
                {
                    anchorObj = GameObject.Instantiate(anchor.Prefab);
                    if (anchorObj != null)
                    {
                        anchorObj.transform.SetParent(parent);
                        if (pose != null)
                        {
                            Pose ps = (Pose)pose;
                            anchorObj.transform.SetLocalPositionAndRotation(ps.position, ps.rotation);
                        }
                        else
                        {
                            anchorObj.transform.SetLocalPositionAndRotation(anchor.DefaultPosition, Quaternion.Euler(anchor.DefaultEulerAngle));
                        }
                        anchorCache.Add(anchorId, anchorObj);
                        anchorObj.GetComponent<IAnchorCreate>()?.OnAnchorCreate();
                    }
                }
            }
            return (anchor, anchorObj);
        }


        public void DestroyAnchor(string anchorId)
        {
            if (anchorCache.TryGetValue(anchorId, out GameObject anchorObj))
            {
                anchorObj.GetComponent<IAnchorDispose>()?.OnAnchorDispose();
                GameObject.Destroy(anchorObj);
            }
        }

        public void SetAnchorActive(string anchorId, bool active)
        {
            if (anchorCache.TryGetValue(anchorId, out GameObject anchorObj))
            {
                if (active)
                {
                    anchorObj.GetComponent<IAnchorShow>()?.OnAnchorShow();
                }
                else
                {
                    anchorObj.GetComponent<IAnchorHide>()?.OnAnchorHide();
                }
                anchorObj.SetActive(active);
            }
        }
    }
}

