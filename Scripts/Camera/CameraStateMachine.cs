using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace JH.Portfolio.Camera
{
    using Camera;

    [DisallowMultipleComponent]
    public class CameraStateMachine : MonoBehaviour
    {
        private const int HORIZINTALCAMERAMODE = (int)CameraMode.FreeView;
        private const int VERTICALCAMERAMODE = (int)CameraMode.TheFirstPersonView | (int)CameraMode.FreeView;

        [Header("Camera information")]
        // Camera mode
        [SerializeField]
        CameraMode _cameraMode = CameraMode.ThirdPersonView;

        public CameraLens cameraLens = CameraLens.Default;

        // Priority for setting camera
        [field: SerializeField] public float CameraPriority { get; set; } = 1;

        [field: Header("Target information")]
        // Reference to target
        [field: SerializeField]
        public Transform followTarget { get; set; } = null;

        // Reference to view direction
        [field: SerializeField] public Transform lookAtTarget { get; set; } = null;

        [Header("Offset of camera")]
        // Offset of camera
        [SerializeField]
        private Vector3 cameraOffset = Vector3.zero;

        // Rotation of camera view
        [FormerlySerializedAs("cameraPositionQuaternion")] [SerializeField]
        private Quaternion directionRotation = Quaternion.identity;

        // The limit of vertical angle
        [MinMaxSlider(-20, 80), SerializeField]
        Vector2 verticalAngleLimit = new Vector2(-180, 180);

        float verticalAngle = 0;

        // The limit of horizontal angle
        [MinMaxSlider(-180, 180), SerializeField]
        Vector2 horizontalAngleLimit = new Vector2(-180, 180);

        float horizontalAngle = 0;


        private void OnEnable()
        {
            if (MainCameraObject.Instance == null) return;

            if (followTarget == null)
            {
                followTarget = transform;
            }

            InitalizeMainCameraEvent();
        }

        private void Start()
        {
            InitalizeMainCameraEvent();
        }

        void InitalizeMainCameraEvent()
        {
            MainCameraObject.Instance.OnChangeCameraEvent += CameraMovement;
        }

        void ClearMainCameraEvent()
        {
            // Null check
            if (MainCameraObject.Instance == null) return;
            MainCameraObject.Instance.OnChangeCameraEvent -= CameraMovement;
        }

        private void OnDisable()
        {
            ClearMainCameraEvent();
        }

        public void CameraMovement(float horizontal, float vertical, ref Vector3 position, ref Quaternion rotation,
            ref float priority, ref CameraLens lens)
        {
            // If this priority is 0, we can't calculate position and rotation
            if (CameraPriority == 0) return;

            #region input value check
            // Check this camera mode is horizontal camera mode
            // if this camera mode is horizontal camera mode, we rotation horizontal angle
            if ((HORIZINTALCAMERAMODE & (int)_cameraMode) != 0)
            {
                horizontalAngle += horizontal;

                // Calculate angle between min and max 
                var min = horizontalAngleLimit.x;
                var max = horizontalAngleLimit.y;
                var angle = horizontalAngle % 180;

                // Limit angle check
                // If angle is between min and max, we can calculate camera direction's rotation
                // else, we calculate camera direction's rotation and clamp horizontalAngle's value between min and max
                if (angle >= min && angle <= max)
                {
                    directionRotation = Quaternion.Euler(0, horizontal, 0) * directionRotation;
                }
                else
                {
                    horizontalAngle = Mathf.Clamp(horizontalAngle, min, max);
                }
            }
            #endregion
            
            // get follow position from follow target object
            var followTargetPosition = followTarget.position;
            // calculate camera distance from camera offset
            var offsetDistance = cameraOffset.magnitude;
            // Calculataion priority
            priority = CameraPriority + priority;
            var t = CameraPriority / priority;

            #region set camera position
            // if the first person view mode, we can't calculate offset position,because camera position is follow target position
            // but another camera mode, we can calculate offset position
            var offsetPosition = Vector3.zero;
            // check this camera mode is vertical camera mode
            if (_cameraMode != CameraMode.TheFirstPersonView)
            {
                // offset position
                offsetPosition = directionRotation * (cameraOffset.normalized * offsetDistance);
            }

            // calculate camera position with follow target position and camera offset
            var cameraPosition = followTargetPosition + offsetPosition;
            // Set Camera Positionww
            position = Vector3.Lerp(position, followTargetPosition + offsetPosition, t);
            #endregion
            #region set camera rotation
            // Set Camera Rotation
            var viewDirection = (lookAtTarget ? lookAtTarget.position - cameraPosition : -offsetPosition).normalized;
            rotation = Quaternion.Lerp(rotation,
                Quaternion.LookRotation(viewDirection.Equals(Vector3.zero) ? followTarget.forward : viewDirection,
                    Vector3.up), t);
            #endregion

            #region set camera lens
            lens.fieldOfView = Mathf.Lerp(lens.fieldOfView, cameraLens.fieldOfView, t);
            lens.ClippingPlane.x = Mathf.Lerp(lens.ClippingPlane.x, cameraLens.ClippingPlane.x, t);
            lens.ClippingPlane.y = Mathf.Lerp(lens.ClippingPlane.y, cameraLens.ClippingPlane.y, t);
            #endregion
        }
    }
}