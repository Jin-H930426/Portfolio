using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace JH.Portfolio.Camera
{
    [DisallowMultipleComponent]
    public class CameraMixer : MonoBehaviour
    {
        [ContextMenuItem("Set list by children","SetListByChildren" )]
        [SerializeField, ReadOnly] private List<CameraStateMachine> cameraStateMachines = new List<CameraStateMachine>();

        /// <summary>
        /// Property value of mix weight
        /// </summary>
        [Range(0, 1), SerializeField] private float mixWeight = 0f;

        /// <summary>
        /// get or set mix weight, for calculate camera priority 
        /// </summary>
        public float MixWeight
        {
            get => mixWeight;
            set
            {
                mixWeight = Mathf.Clamp01(value);
                CalculateWight();
            }
        }

        private void Start()
        {
            CalculateWight();
            SetListByChildren();
        }
        
        /// <summary>
        /// Calculate camera priority by mix weight
        /// </summary>
        private void CalculateWight()
        {
            int count = cameraStateMachines.Count;
            if (count == 0) return;
            if (count == 1)
            {
                cameraStateMachines[0].CameraPriority = 1;
                return;
            }
            
            int machineIndex = Mathf.FloorToInt(mixWeight * --count);
            float weight = mixWeight * count - machineIndex;
            
            SetCameraPriority(machineIndex, weight);            
        }
        /// <summary>
        /// Clear all cameras state machines priority
        /// </summary>
        void ClearCameraPriority()
        {
            foreach (var cameraStateMachine in cameraStateMachines)
            {
                cameraStateMachine.CameraPriority = 0;
            }
        }
        /// <summary>
        /// Set target camera state machine priority by index and weight
        /// </summary>
        /// <param name="index">state machine index</param>
        /// <param name="weight">state machine priority</param>
        void SetCameraPriority(int index, float weight)
        {
            if (index < 0 || index >= cameraStateMachines.Count) return;
            ClearCameraPriority();
            
            cameraStateMachines[index].CameraPriority = 1 - weight;
            if(index + 1 < cameraStateMachines.Count) cameraStateMachines[index + 1].CameraPriority = weight;
        }
        /// <summary>
        /// Set camera state machine list by children
        /// </summary>
        public void SetListByChildren()
        {
            cameraStateMachines.Clear();
            foreach (var machine in GetComponentsInChildren<CameraStateMachine>(true))
            {
                cameraStateMachines.Add(machine);
            }
        }
        #if UNITY_EDITOR
        private void OnValidate()
        {
            CalculateWight();
        }
        #endif
    }
}