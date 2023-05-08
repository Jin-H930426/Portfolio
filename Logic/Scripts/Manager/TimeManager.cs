using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace JH.Portfolio.Manager
{
    [System.Serializable]
    public class TimeManager
    {
        #region Define 
        // Max time for interval
        const float MAX_TIME = 86400f; // one day second (24 * 60 * 60)
        /// <summary>
        /// type of interval
        /// user can select interval type for each time event
        /// ex).. world : in game time, if game is paused, time is not running
        /// .....    ui : ui runtime, always running even when game is paused
        /// </summary>
        public enum IntervalType
        {
            World,
            UI
        } 
        #endregion
        #region Variable 
        // Singleton
        private static TimeManager _instance;
        #if UNITY_EDITOR // for view test data on Unity Editor 
        // Dictionary for time event
        [SerializeField] private SerializedDictionary<string, float> _intervalTriggers  = new SerializedDictionary<string, float>();
        [SerializeField] private SerializedDictionary<string, float> _uiIntervalTriggers = new SerializedDictionary<string, float>();
        [SerializeField] private SerializedDictionary<string, Action> _intervalActions = new SerializedDictionary<string, Action>();
        #else 
        private Dictionary<string, float> _intervalTriggers  = new Dictionary<string, float>();
        private Dictionary<string, float> _uiIntervalTriggers = new Dictionary<string, float>();
        private Dictionary<string, Action> _intervalActions = new Dictionary<string, Action>();
        #endif
        // Remove queue
        private Queue<(string key, IntervalType type)> _removeQueue = new Queue<(string, IntervalType)>();
        #endregion
        #region Property
        public static TimeManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TimeManager();
                }
                return _instance;
            }
        }
        // Delta Time
        /// <summary>
        /// World Delta Time
        /// </summary>
        public static float DeltaTime { get; private set; } = 0f;
        /// <summary>
        /// UI Delta Time
        /// </summary>
        public static float DeltaTimeUI { get; private set; } = 0f;
        // acount world time
        public float worldTime { get; private set; } = 0;
        public float worldTimeUI { get; private set; } = 0;
        // Time scale
        public float TimeScale { get; set; } = 1f;
        public float UITimeSacle { get; set; } = 1f;
        #endregion
        
        /// <summary>
        /// Update time by deltaTime
        /// </summary>
        /// <param name="deltaTime"></param>
        public void Update(float deltaTime)
        {
            // Interval Check
            GameTimeTriggerUpdate(deltaTime);
            UITimeTriggerUpdate(deltaTime);
            // Remove time event
            RemoveTimeTrigger();
        }

      
        void GameTimeTriggerUpdate(float deltaTime)
        {
            DeltaTime = deltaTime * TimeScale;
            worldTime += DeltaTime;
            bool overflow = worldTime > MAX_TIME;
            
            // Update time event
            #region old 
            foreach (var intervalTrigger in _intervalTriggers)
            {
                if (intervalTrigger.Value > 0 && intervalTrigger.Value < worldTime)
                    _removeQueue.Enqueue((intervalTrigger.Key, IntervalType.World));
                if (overflow) _intervalTriggers[intervalTrigger.Key] = intervalTrigger.Value - MAX_TIME;
            }
            #endregion
            
            if (overflow) worldTime -= MAX_TIME;

        }
        void UITimeTriggerUpdate(float deltaTime)
        {
            DeltaTimeUI = deltaTime * UITimeSacle;
            worldTimeUI += DeltaTimeUI;
            bool overflow = worldTimeUI > MAX_TIME;
            
            // Update time event
            foreach (var intervalTrigger in _uiIntervalTriggers)
            {
                if (intervalTrigger.Value > 0 && intervalTrigger.Value < worldTime)
                    _removeQueue.Enqueue((intervalTrigger.Key, IntervalType.UI));
                if (overflow) _uiIntervalTriggers[intervalTrigger.Key] = intervalTrigger.Value - MAX_TIME;
            } 
            if (overflow) worldTimeUI -= MAX_TIME;

        }
        void RemoveTimeTrigger()
        {
            while (_removeQueue.Count > 0)
            {
                var remove = _removeQueue.Dequeue();
                
                if (_intervalActions.ContainsKey(remove.key))
                {
                    _intervalActions[remove.key].Invoke();
                    _intervalActions.Remove(remove.key);
                } 
                
                switch (remove.type)
                {
                    case IntervalType.World:
                        _intervalTriggers.Remove(remove.key);
                        break;
                    default:
                        _intervalTriggers.Remove(remove.key);
                        break;
                }
            } 
        }
        
        /// <summary>
        /// Add time event
        /// if interval is less than 0, event will be removed
        /// </summary>
        /// <param name="key">event key</param>
        /// <param name="interval">action delay</param>
        /// <param name="action">action</param>
        public static bool AddTimeEvent(string key, float interval, Action action)
        {
            // If key is already exist, return
            if (!AddTimeEvent(key, interval)) return false;
            // Add time event
            _instance._intervalActions.Add(key, action);
            return true;
        }
        public static bool AddTimeEvent(string key, float interval)
        {
            // If key is already exist, return
            if (_instance._intervalTriggers.ContainsKey(key))
                return false;
            // Add time event
            _instance._intervalTriggers.Add(key, interval + _instance.worldTime);
            return true;
        }
        
        /// <summary>
        /// Add ui time event
        /// </summary>
        /// <param name="key">event key</param>
        /// <param name="interval">action delay</param>
        /// <param name="action">action</param>
        public static bool AddUITimeEvent(string key, float interval, Action action)
        {
            // If key is already exist, return
            if (!AddUITimeEvent(key, interval)) return false;
            // Add time event
            _instance._intervalActions.Add(key, action);
            return true;
        }
        public static bool AddUITimeEvent(string key, float interval)
        {
            // If key is already exist, return
            if (_instance._uiIntervalTriggers.ContainsKey(key))
                return false;
            // Add time event
            _instance._uiIntervalTriggers.Add(key, interval + _instance.worldTime);
            return true;
        }
        
        /// <summary>
        /// Remove time event
        /// </summary>
        /// <param name="key">event key for remove event</param>
        public static bool RemoveTimeEvent(string key)
        {
            if (_instance._intervalActions.ContainsKey(key))
                _instance._intervalActions.Remove(key);
            // If key is exist, remove event
            if (_instance._intervalTriggers.ContainsKey(key))
            {
                _instance._intervalTriggers.Remove(key);
                return true;
            }
            return false;
        }
        /// <summary>
        /// Remove ui time event
        /// </summary>
        /// <param name="key">event key for remove event</param>
        public static bool RemoveUITimeEvent(string key)
        {
            if (_instance._intervalActions.ContainsKey(key))
                _instance._intervalActions.Remove(key);
            // If key is exist, remove event
            if (_instance._uiIntervalTriggers.ContainsKey(key))
            {
                _instance._uiIntervalTriggers.Remove(key);
                return true;
            }
            return false;
        }
        /// <summary>
        /// Get time event remain time
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static float GetRemainingTime(string key)
        {
            if (_instance._intervalTriggers.ContainsKey(key))
                return _instance._intervalTriggers[key] - _instance.worldTime;
            return -1.0f;
        }
        /// <summary>
        /// Clear all time event
        /// </summary>
        public static void ClearTimeEvent()
        {
            _instance._intervalTriggers.Clear();
        }
        /// <summary>
        /// Destroy time manager
        /// </summary>
        public void Destroy()
        {
            ClearTimeEvent();
            _intervalTriggers = null;
        }
    }
}