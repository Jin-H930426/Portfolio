using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace JH.Portfolio.Camera
{
    using Manager;
    [DefaultExecutionOrder(-1)]
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class CameraBrain : MonoBehaviour
    {
        // Camera brain dictionary for find camera brain
        static Dictionary<string, CameraBrain> cameraBrains;
        // Camera brain name for Add to dictionary
        public string cameraBrainName = "MainCamera";
        UnityEngine.Camera _camera;
        // Camera handler for change camera from machines
        public delegate void ChangeCameraHandler(float horizontal, float vertical,ref Vector3 position, ref Quaternion rotation, ref float priority, ref CameraLens lens); 
        public event ChangeCameraHandler OnChangeCameraEvent;
        // Input values
        private float _horizontal = 0;
        private float _vertical = 0;
        
        private void Awake()
        {
            if (cameraBrains == null) cameraBrains = new Dictionary<string, CameraBrain>();
            if (cameraBrains.ContainsKey(cameraBrainName))
            {
                Debug.LogWarning($"Destroy {name} Object, because MainCameraObject is already exist : {cameraBrainName}");
                Destroy(gameObject);
                return;
            }
            
            cameraBrains.Add(cameraBrainName, this);
            _camera = GetComponent<UnityEngine.Camera>();
            DontDestroyOnLoad(gameObject);
        }
        private void OnEnable()
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
        
        /// <summary>
        /// Find camera brain from dictionary
        /// </summary>
        /// <param name="cameraBrainName">for finding key</param>
        /// <returns></returns>
        public static CameraBrain GetCameraBrain(string cameraBrainName)
        {
            if (cameraBrains == null) return null;
            if (!cameraBrains.ContainsKey(cameraBrainName)) return null;
            return cameraBrains[cameraBrainName];
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
        /// <summary>
        /// Set camera position and rotation
        /// </summary>
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
        /// <summary>
        /// Set camera position and rotation immediately
        /// </summary>
        /// <param name="position">setting position</param>
        /// <param name="rotation">setting rotation</param>
        /// <param name="lens">setting lens value</param>
        public void SetCameraImediately(Vector3 position, Quaternion rotation, CameraLens lens)
        {
            _camera.fieldOfView = lens.fieldOfView;
            _camera.nearClipPlane = lens.ClippingPlane.x;
            _camera.farClipPlane = lens.ClippingPlane.y;
            
            transform.SetPositionAndRotation(position, rotation);
        }
        /// <summary>
        /// Add input values
        /// </summary>
        /// <param name="positionInput"></param>
        /// <param name="rotationInput"></param>
        private void OnCameraMovement(Vector3 positionInput, Vector3 rotationInput)
        {
            if (!enabled)
            {
                return;
            }
            
            _horizontal += rotationInput.x;
            _vertical += rotationInput.y;
        }
        
        
        #if UNITY_EDITOR
        /// <summary>
        /// Set camera position and rotation in editor
        /// </summary>
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
        /// <summary>
        /// Set camera position and rotation immediately in editor
        /// </summary>
        /// <param name="position">setting position</param>
        /// <param name="rotation">setting rotation</param>
        /// <param name="lens">setting lens value</param>
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