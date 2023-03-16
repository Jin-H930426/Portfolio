using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JH.Portfolio.Manager
{
    [DefaultExecutionOrder(10)]
    public class GameManager : MonoBehaviour
    {
        // Reference GameManager component for Singleton
        public static GameManager Instance { get; private set; }
        // Reference to other managers
        [ReadOnly, SerializeField] private TimeManager _timeManager;
        public static TimeManager TimeManager => Instance._timeManager;
        [ReadOnly, SerializeField] private InputManager _inputManager;
        // public static InputManager InputManager => instance._inputManager;
        public static InputManager InputManager => Instance?._inputManager;
        public InputManager.InputType InputType = InputManager.InputType.Keyboard;
        
        // Initialize game manager
        private void Awake()
        {
            // Initialize Singleton
            // If there is yet an instance of GameManager in the scene, set it to this one
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            // If there is already an instance of GameManager, destroy this one
            else
            {
                Destroy(gameObject);
                return;
            }
            // Platform Check
            #region Platform Check
#if PLATFORM_ANDROID || PLATFORM_IOS
            if (InputType == InputManager.InputType.Keyboard)
                InputType = InputManager.InputType.Mobile;
#elif PLATFORM_STANDALONE
            if (InputType == InputManager.InputType.Mobile)
                InputType = InputManager.InputType.Keyboard;
#endif
            #endregion 
            _timeManager = new TimeManager();
            _inputManager = new InputManager(InputType);
        }
        // game manager update
        private void Update()
        {
            UnityEngine.Profiling.Profiler.BeginSample("GameManager.Update");
            var deltaTime = Time.deltaTime;
            _timeManager.Update(deltaTime);
            _inputManager.Update(deltaTime);
            UnityEngine.Profiling.Profiler.EndSample();
        }
        // On destroy game manager component  
        private void OnDestroy()
        {
            _timeManager.Destroy();
            _inputManager.Destroy();
            _timeManager = null;
            _inputManager = null;
        }


    }
}

