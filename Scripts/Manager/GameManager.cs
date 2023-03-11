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
        public TimeManager timeManager;
        
        
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
            
            timeManager = new TimeManager();
        }

        private void Start()
        {
           timeManager.AddTimeEvent("Loop test", 1f, true, () => { Debug.Log("Loop test"); }); 
           timeManager.AddTimeEvent("test", 1f, false, () => { Debug.Log("test"); }); 
        }

        private void Update()
        {
            if(timeManager == null) return;
            timeManager.Update(Time.deltaTime);
        }
        
        private void OnDestroy()
        {
            timeManager.ClearTimeEvent();
            timeManager = null;
        }
    }
}

