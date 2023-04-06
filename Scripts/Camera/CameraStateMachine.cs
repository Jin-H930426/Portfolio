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
        public Vector3 CameraPosition { get; private set; }
        public Vector3 CameraDirection { get; private set; }
        public Quaternion CameraRotation { get; private set; }

        [Header("Offset of camera")]
        // Offset of camera
        [SerializeField]
        private Vector3 cameraOffset = Vector3.zero;

        // Rotation of camera view
        private Quaternion horizontalRotation = Quaternion.identity;
        private Quaternion verticalRotation = Quaternion.identity;
       
        [Header("Limit of camera angle")]
        // The limit of horizontal angle
        [MinMaxSlider(-180, 180), SerializeField]
        Vector2 horizontalAngleLimit = new Vector2(-180, 180);
        float _horizontalAngle = 0;
        
        // The limit of vertical angle
        [MinMaxSlider(-20, 120), SerializeField]
        Vector2 verticalAngleLimit = new Vector2(-180, 180);
        float _verticalAngle = 0;


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
        private void OnDisable()
        {
            ClearMainCameraEvent();
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


        public void CameraMovement(float horizontal, float vertical, ref Vector3 position, ref Quaternion rotation,
            ref float priority, ref CameraLens lens)
        {
            // If this priority is 0, we can't calculate position and rotation
            if (CameraPriority == 0) return;
            CalculationPosition(horizontal);
            CalculationRotation(vertical);
            
            // Calculataion priority
            priority = CameraPriority + priority;
            var t = CameraPriority / priority;
            
            // Set world camera position, rotation and lens
            position = Vector3.Lerp(position, CameraPosition, t);
            rotation = Quaternion.Lerp(rotation, CameraRotation, t);
            lens = CameraLens.Lerp(lens, cameraLens, t);
        }
        
        public void CalculationPosition(float horizontal)
        {
            // Check this camera mode is horizontal camera mode
            // if this camera mode is horizontal camera mode, we rotation horizontal angle
            if ((HORIZINTALCAMERAMODE & (int)_cameraMode) != 0)
            {
                _horizontalAngle += horizontal;

                // Calculate angle between min and max 
                var min = horizontalAngleLimit.x;
                var max = horizontalAngleLimit.y;
                var angle = _horizontalAngle % 180;

                // Limit angle check
                // If angle is between min and max, we can calculate camera direction's rotation
                // else, we calculate camera direction's rotation and clamp horizontalAngle's value between min and max
                if (angle >= min && angle <= max)
                {
                    horizontalRotation = Quaternion.Euler(0, horizontal, 0) * horizontalRotation;
                }
                else
                {
                    _horizontalAngle = Mathf.Clamp(_horizontalAngle, min, max);
                }
            }
            
            
            // get follow position from follow target object
            var followTargetPosition = followTarget.position;
            // calculate camera distance from camera offset
            var offsetDistance = cameraOffset.magnitude;
            

            #region set camera position
            // if the first person view mode, we can't calculate offset position,because camera position is follow target position
            // but another camera mode, we can calculate offset position
            var offsetPosition = Vector3.zero;
            // check this camera mode is vertical camera mode
            if (_cameraMode != CameraMode.TheFirstPersonView)
            {
                // offset position
                offsetPosition = horizontalRotation * (cameraOffset.normalized * offsetDistance);
            }

            // calculate camera position with follow target position and camera offset
            CameraPosition = followTargetPosition + offsetPosition;
            CameraDirection = lookAtTarget ? lookAtTarget.position - CameraPosition : -cameraOffset;
            #endregion
        }
        public void CalculationRotation(float vertical)
        {
            var viewDirection = CameraDirection.normalized;
            // get view direction from look at target and offset;
            // Check this camera mode is vertical camera mode
            if ((VERTICALCAMERAMODE & (int)_cameraMode) != 0)
            {
                // calculate vertical angle
                _verticalAngle += vertical;
                var min = verticalAngleLimit.x;
                var max = verticalAngleLimit.y;
                var angle = _verticalAngle % 180;
                // Limit angle check
                if (angle >= min && angle <= max)
                {
                    verticalRotation = Quaternion.Euler(-vertical, 0, 0) * verticalRotation;
                }
                else
                {
                    _verticalAngle = Mathf.Clamp(_verticalAngle, min, max);
                }
            }
            
            // setting machine camera rotation
            CameraRotation =  Quaternion.LookRotation(viewDirection.Equals(Vector3.zero) ?
                followTarget.forward : // if view direction is zero, we use follow target's forward
                viewDirection, Vector3.up); // else, we use view direction
            
            CameraRotation = CameraRotation * verticalRotation;
            // calculate camera rotation
        }
    }
}