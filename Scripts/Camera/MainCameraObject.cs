using System;
using System.Diagnostics;
using UnityEngine;

namespace JH.Portfolio.Camera
{
    using Manager;
    using Camera;
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class MainCameraObject : MonoBehaviour
    {
        public static MainCameraObject Instance { get; private set; }
        UnityEngine.Camera _camera;
        public delegate void ChangeCameraHandler(float horizontal, float vertical,ref Vector3 position, ref Quaternion rotation, ref float priority, ref CameraLens lens); 
        public event ChangeCameraHandler OnChangeCameraEvent;
        // Input values
        private float _horizontal = 0;
        private float _vertical = 0;
        
        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning($"Destroy {name} Object, because MainCameraObject is already exist : {Instance.name}");
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            _camera = GetComponent<UnityEngine.Camera>();
            DontDestroyOnLoad(gameObject);
        }
        private void OnEnable()
        {
            // Null check for initialize
            if (GameManager.InputManager == null) return;
            
            InitializeInput();
        }
        private void Start()
        {
            InitializeInput();
        }
        private void OnDisable()
        {
            ClearInput();
        }
        private void OnDestroy()
        {
            OnChangeCameraEvent = null;
        }
        // Add input event
        private void InitializeInput()
        {
            GameManager.InputManager.OnRotNMoveInputEvent.OnHeldEvent += OnCameraMovement;
        }
        // Remove input event
        private void ClearInput()
        {
            // Null check
            if (GameManager.InputManager == null) return;
            
            GameManager.InputManager.OnRotNMoveInputEvent.OnHeldEvent -= OnCameraMovement;
        }
        // Camera Update
        private void LateUpdate()
        {
            SetPositionAndRotation();
        }
        // Set camera position and rotation
        public void SetPositionAndRotation()
        {
            UnityEngine.Profiling.Profiler.BeginSample("MainCameraObject.SetPositionAndRotation");
            float priority = 0;
            var position = Vector3.zero;
            var rotation = Quaternion.identity;
            var cameraLens = CameraLens.Default;
            
            OnChangeCameraEvent?.Invoke(_horizontal, _vertical, ref position, ref rotation, ref priority, ref cameraLens);
            if (priority == 0) return;
            
            // Set camera position and rotation
            SetCameraImediately(position, rotation, cameraLens);
            
            // Reset inputs
            _horizontal = 0;
            _vertical = 0;
            UnityEngine.Profiling.Profiler.EndSample();
        }
        // set imediatly camera position.
        public void SetCameraImediately(Vector3 position, Quaternion rotation, CameraLens lens)
        {
            _camera.fieldOfView = lens.fieldOfView;
            _camera.nearClipPlane = lens.ClippingPlane.x;
            _camera.farClipPlane = lens.ClippingPlane.y;
            
            transform.SetPositionAndRotation(position, rotation);
        }
        // add input value
        private void OnCameraMovement(Vector3 positionInput, Vector3 rotationInput)
        {
            UnityEngine.Profiling.Profiler.BeginSample("MainCameraObject.OnCameraMovement");
            if (!enabled)
            {
                return;
            }
            
            _horizontal += rotationInput.x;
            _vertical += rotationInput.y;
            UnityEngine.Profiling.Profiler.EndSample();
        }

        
        #if UNITY_EDITOR
        public void SetPositionAndRotationAtEditor()
        {
            if (Application.isPlaying) return;
            float priority = 0;
            var position = Vector3.zero;
            var rotation = Quaternion.identity;
            var cameraLens = CameraLens.Default;
            
            foreach (var machine in FindObjectsOfType<CameraStateMachine>())
            {
                machine.CameraMovement(0, 0, ref position, ref rotation, ref priority, ref cameraLens);
            }
            
            transform.SetPositionAndRotation(position, rotation);
            
            var camera = GetComponent<UnityEngine.Camera>();
            
            camera.fieldOfView = cameraLens.fieldOfView;
            camera.nearClipPlane = cameraLens.ClippingPlane.x;
            camera.farClipPlane = cameraLens.ClippingPlane.y;
        }
        public void SetCameraImediatelyAtEditor(Vector3 position, Quaternion rotation, CameraLens lens)
        {
            var camera = GetComponent<UnityEngine.Camera>();
            
            camera.fieldOfView = lens.fieldOfView;
            camera.nearClipPlane = lens.ClippingPlane.x;
            camera.farClipPlane = lens.ClippingPlane.y;
            
            transform.SetPositionAndRotation(position, rotation);
        }
        #endif
    }
}