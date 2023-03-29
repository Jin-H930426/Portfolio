using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

namespace JH.Portfolio.Controller
{
    using Manager;
    
    [RequireComponent(typeof(Camera))]
    public class MainCameraObject : MonoBehaviour
    {
        public static MainCameraObject Instance { get; private set; }
        public delegate void ChangeCameraHandler(float horizontal, float vertical,ref Vector3 position, ref Quaternion rotation, ref float priority); 
        public event ChangeCameraHandler OnChangeCameraEvent;

        Vector3 _cameraPosition;
        Quaternion _cameraQuaternion;
        
        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning($"Destroy {name} Object, because MainCameraObject is already exist : {Instance.name}");
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
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
        private void InitializeInput()
        {
            GameManager.InputManager.OnRotNMoveInputEvent.OnHeldEvent += OnCameraMovement;
        }
        private void ClearInput()
        {
            // Null check
            if (GameManager.InputManager == null) return;
            
            GameManager.InputManager.OnRotNMoveInputEvent.OnHeldEvent -= OnCameraMovement;
        }
        private void LateUpdate()
        {
            transform.SetPositionAndRotation(_cameraPosition, _cameraQuaternion);
        }

        private void OnCameraMovement(Vector3 positionInput, Vector3 rotationInput)
        {
            if (!enabled)
            {
                return;
            }
            
            var horizontal = rotationInput.x;
            var vertical = rotationInput.y;
            float priority = 0;
            var position = Vector3.zero;
            var rotation = Quaternion.identity;
            
            OnChangeCameraEvent?.Invoke(horizontal, vertical, ref position, ref rotation, ref priority);
            if (priority == 0) return;
            
            _cameraPosition = position;
            _cameraQuaternion = rotation;
        }
        
    }
}