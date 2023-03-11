using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JH.Portfolio.Manager
{
    public class GameManager : MonoBehaviour
    {
        // Reference GameManager component for Singleton
        public static GameManager instance;
        
        // Reference to other managers
        private TimeManager _timeManager;
        
        private void Awake()
        {
            // Initialize Singleton
            // If there is yet an instance of GameManager in the scene, set it to this one
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            // If there is already an instance of GameManager, destroy this one
            else
            {
                Destroy(gameObject);
                return;
            }
            
            _timeManager = new TimeManager();
        }
        private void Update()
        {
            if(_timeManager == null) return;
            _timeManager.Update(Time.deltaTime);
        }
        
        private void OnDestroy()
        {
            _timeManager.ClearTimeEvent();
            _timeManager = null;
        }
    }
}

