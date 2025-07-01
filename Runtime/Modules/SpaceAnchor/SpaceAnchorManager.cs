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


        public SpaceAnchor CreateAnchor(string anchorId, Transform parent = null, Pose? pose = null)
        {
            var info = config.GetAnchorInfo(anchorId);
            if (!anchorCache.TryGetValue(anchorId, out GameObject anchorObj))
            {
                if (info != null)
                {
                    anchorObj = GameObject.Instantiate(info.Prefab);
                    if (anchorObj != null)
                    {
                        anchorObj.transform.SetParent(parent);
                        if (pose != null)
                        {
                            var ps = pose.Value;
                            anchorObj.transform.SetLocalPositionAndRotation(ps.position, ps.rotation);
                        }
                        else
                        {
                            anchorObj.transform.SetLocalPositionAndRotation(info.DefaultPosition, Quaternion.Euler(info.DefaultEulerAngle));
                        }
                        anchorCache.Add(anchorId, anchorObj);
                        anchorObj.GetComponent<IAnchorCreate>()?.OnAnchorCreate();
                    }
                }
            }
            return new SpaceAnchor(anchorId, info, anchorObj);
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

