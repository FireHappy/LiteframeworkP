using UnityEngine;
using UnityEngine.XR;

namespace LiteFramework.Components
{
    public class FollowTarget : MonoBehaviour
    {
        public enum FollowType
        {
            RotationAndPosition,
            PositionOnly,
            RotationOnly
        }

        public enum UpdateType
        {
            /// <summary>
            /// 更新阶段和渲染前都进行更新
            /// </summary>
            UpdateAndBeforeRender,
            /// <summary>
            /// 仅在 Update 阶段更新
            /// </summary>
            Update,
            /// <summary>
            /// 仅在渲染前更新（例如 XR）
            /// </summary>
            BeforeRender,
        }

        [Header("跟随设置")]
        public Transform target;
        public FollowType followType = FollowType.RotationAndPosition;
        public UpdateType updateType = UpdateType.UpdateAndBeforeRender;
        public Vector3 positionOffset = Vector3.zero;
        public Vector3 rotationOffset = Vector3.zero;

        [Header("平滑设置")]
        public bool smoothFollow = true;
        public float positionLerpSpeed = 5f;
        public float rotationLerpSpeed = 5f;

        private void OnEnable()
        {
            Application.onBeforeRender += OnBeforeRenderCallback;
        }

        private void OnDisable()
        {
            Application.onBeforeRender -= OnBeforeRenderCallback;
        }

        private void Update()
        {
            if (updateType == UpdateType.Update || updateType == UpdateType.UpdateAndBeforeRender)
            {
                PerformFollow();
            }
        }

        private void OnBeforeRenderCallback()
        {
            if (updateType == UpdateType.BeforeRender || updateType == UpdateType.UpdateAndBeforeRender)
            {
                PerformFollow();
            }
        }

        private void PerformFollow()
        {
            if (target == null) return;

            switch (followType)
            {
                case FollowType.PositionOnly:
                    FollowPosition();
                    break;
                case FollowType.RotationOnly:
                    FollowRotation();
                    break;
                case FollowType.RotationAndPosition:
                    FollowPosition();
                    FollowRotation();
                    break;
            }
        }

        private void FollowPosition()
        {
            Vector3 targetPosition = target.position + target.TransformDirection(positionOffset);

            if (smoothFollow)
            {
                transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * positionLerpSpeed);
            }
            else
            {
                transform.position = targetPosition;
            }
        }

        private void FollowRotation()
        {
            Quaternion targetRotation = target.rotation * Quaternion.Euler(rotationOffset);

            if (smoothFollow)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationLerpSpeed);
            }
            else
            {
                transform.rotation = targetRotation;
            }
        }
    }

}
