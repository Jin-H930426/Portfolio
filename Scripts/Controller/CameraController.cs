using System;
using System.Collections;
using System.Collections.Generic;
using JH.Portfolio.Manager;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace JH.Portfolio.Controller
{
    public class CameraController : MonoBehaviour
    {
        private const int HORIZINTALCAMERAMODE = (int)CameraMode.FreeView;
        private const int VERTICALCAMERAMODE = (int)CameraMode.TheFirstPersonView | (int)CameraMode.FreeView;

        [Serializable]
        public enum CameraMode : int
        {
            TheFirstPersonView = 1,
            ThirdPersonView = 2,
            FreeView = 4
        }

        [SerializeField] CameraMode _cameraMode = CameraMode.ThirdPersonView;

        // Reference to target
        [field: SerializeField] public Transform Target { get; set; } = null;

        // Offset of camera
        [SerializeField] private Vector3 cameraOffset = Vector3.zero;
        // Rotation of camera view
        [SerializeField] private Quaternion cameraRotation = Quaternion.identity;

        // Priority for setting camera
        [field: SerializeField] public float CameraPriority { get; set; } = 1;

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

        void CameraMovement(float horizontal, float vertical, ref Vector3 position, ref Quaternion rotation, ref float priority)
        {
            // If this priority is 0, we can't calculate position and rotation
            if (CameraPriority == 0) return;

            // Calculate camera direction from target position
            if ((HORIZINTALCAMERAMODE & (int)_cameraMode) != 0)
            { 
                horizontalAngle += horizontal;
                
                var min = horizontalAngleLimit.x;
                var max = horizontalAngleLimit.y;
                var angle = horizontalAngle % 180;
                if (angle >= min && angle <= max)
                {
                    cameraRotation = Quaternion.Euler(0, horizontal, 0) * cameraRotation;
                }
                else if (angle < min)
                {
                    horizontalAngle = min;
                }
                else if (angle > max)
                {
                    horizontalAngle = max;
                }
            }

            var targetPosition = Target.position;
            // default distance from target
            var distance = cameraOffset.magnitude;
            // Calculate camera direction from target position
            var direction = cameraRotation * (cameraOffset.normalized * distance);
            var viewDirection = -direction;
            // If camera mode is TheFirstPersonView, we can't calculate camera position
            if (_cameraMode == CameraMode.TheFirstPersonView)
            {
                direction = Vector3.zero;
                viewDirection = cameraRotation * Target.forward;
            }
            // Calculataion priority
            priority = CameraPriority + priority;
            var t = CameraPriority / priority;
            // Set Camera Positionww
            position = Vector3.Lerp(position, targetPosition + direction, t);
            // Set Camera Rotation
            rotation = Quaternion.Lerp(rotation, Quaternion.LookRotation(viewDirection, Vector3.up), t);
        }
    }
}