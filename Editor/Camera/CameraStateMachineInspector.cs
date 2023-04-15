using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JH.Portfolio.Camera
{
    [CustomEditor(typeof(CameraStateMachine))]
    public class CameraStateMachineInspector : Editor
    {
        private bool _isInScene = false;
        CameraBrain _cameraBrain;
        
        private void OnEnable()
        {
            _isInScene = JHUtility.IsInScene(target as CameraStateMachine);
            
            if (!_isInScene) return;
            
            _cameraBrain = FindObjectOfType<CameraBrain>();
            SetCamera();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            // If runtime, return
            if (Application.isPlaying || !_isInScene) return;
            SetCamera();
        }
        void SetCamera()
        {
            // Get target as camera state machine
            if (target is not CameraStateMachine machine) return;
            
            bool isFollowTarget = machine.followTarget;
            var savedFollowTarget = machine.followTarget;
            
            if (!isFollowTarget) machine.followTarget = machine.transform;
            
            // Reference camera position, rotation and priority
            var position = Vector3.zero;
            var rotation = Quaternion.identity;
            var priority = 0f;
            var cameraLens = CameraLens.Default;
            // Save machine's priority
            var savePriority = machine.CameraPriority;
            // Set machine's priority to 1 for calculate camera position and rotation 
            machine.CameraPriority = 1;
            // Calculate camera position and rotation
            machine.CameraMovement(0, 0, ref position, ref rotation, ref priority, ref cameraLens);
            // Set camera position and rotation
            _cameraBrain.SetCameraImediatelyAtEditor(position, rotation, machine.cameraLens );
            
            // Load machine's priority from saved value
            machine.CameraPriority = savePriority;
            machine.followTarget = savedFollowTarget;
        }
    }
}