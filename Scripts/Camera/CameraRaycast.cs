using System;
using UnityEngine;
using UnityEngine.Events;

namespace JH.Portfolio.Camera
{
    [System.Serializable]
    public class CameraRaycast : MonoBehaviour
    {
        [Header("For ray setting"), Space, SerializeField] CameraStateMachine _cameraStateMachine;
        public LayerMask raycastMask;
        
        public Vector3 CameraPosition => _cameraStateMachine.CameraPosition;
        public Vector3 CameraDirection => _cameraStateMachine.CameraDirection.normalized;
        public Ray CameraRay => new Ray(CameraPosition, CameraDirection);
        public float RaycastDistance => _cameraStateMachine.CameraDirection.magnitude;
        
        private RaycastHit _hit;
        
        private bool _isEnter = false;
        [Header("Raycast Events"),Space]
        public UnityEvent<RaycastHit> onEnterRaycastHit;
        public UnityEvent onExitRaycastHit;

        public void Start()
        {
            if (!_cameraStateMachine)
            {
                Debug.LogError("Camera state machine not found");
                enabled = false;
            }
        }
        private void Update()
        {
            if (!_cameraStateMachine) return;
            if (_cameraStateMachine.CameraPriority == 0) _cameraStateMachine.CalculationPosition(0);
            bool newState = Physics.Raycast(CameraRay, out _hit, RaycastDistance, raycastMask); 
            if (!_isEnter &&  newState)
            {
                Debug.DrawRay(Vector3.positiveInfinity, CameraDirection * RaycastDistance, Color.green, 1);
                onEnterRaycastHit?.Invoke(_hit);
            }
            else if (newState)
            {
                Debug.DrawRay(Vector3.positiveInfinity, CameraDirection * RaycastDistance, Color.green, 1);
            }
            else
            {
                Debug.DrawRay(Vector3.positiveInfinity, CameraDirection * RaycastDistance, Color.red, 1);
                onExitRaycastHit?.Invoke();
            }
        }
    }
}