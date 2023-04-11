using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace JH.Portfolio.Manager
{
    [System.Serializable]
    public class TimeManager
    {
        #region Define 
        const float MAX_TIME = 1000000f;
        public enum IntervalType
        {
            World,
            UI
        } 
        #endregion

        #region Variable 
        // Dictionary for time event
        private Dictionary<string, float> _intervalTriggers { get; set; } = new Dictionary<string, float>();
        private Dictionary<string, float> _uiIntervalTriggers { get; set; } = new Dictionary<string, float>();
        private Dictionary<string, Action> _intervalActions { get; set; } = new Dictionary<string, Action>();
        // Remove queue
        private Queue<(string key, IntervalType type)> removeQueue { get; set; } = new Queue<(string, IntervalType)>();
        #endregion

        #region Property
        // Delta Time
        public static float DeltaTime { get; private set; } = 0f;
        public static float DeltaTimeUI { get; private set; } = 0f;
        // World time
        [field: SerializeField] public float worldTime { get; private set; } = 0;
        [field: SerializeField] public float worldTimeUI { get; private set; } = 0;
        // Time scale
        [field: SerializeField] public float TimeScale { get; set; } = 1f;
        [field: SerializeField] public float UITimeSacle { get; set; } = 1f;
        #endregion
        
        /// <summary>
        /// Update time by deltaTime
        /// </summary>
        /// <param name="deltaTime"></param>
        public void Update(float deltaTime)
        {
            UnityEngine.Profiling.Profiler.BeginSample("TimeManager.Update");
            // Interval Check
            GameTimeTriggerUpdate(deltaTime);
            GameTimeTriggerUpdate(deltaTime);
            
            // Remove time event
            RemoveTimeTrigger();
            UnityEngine.Profiling.Profiler.EndSample();
        }

        void GameTimeTriggerUpdate(float deltaTime)
        {
            DeltaTime = deltaTime * TimeScale;
            worldTime += DeltaTime;
            bool overflow = worldTime > MAX_TIME;
            
            // Update time event
            foreach (var intervalTrigger in _intervalTriggers)
            {
                if (intervalTrigger.Value < worldTime)
                    removeQueue.Enqueue((intervalTrigger.Key, IntervalType.World));
                if (overflow) _intervalTriggers[intervalTrigger.Key] = intervalTrigger.Value - MAX_TIME;
            }
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
                if (intervalTrigger.Value < worldTime)
                    removeQueue.Enqueue((intervalTrigger.Key, IntervalType.UI));
                if (overflow) _uiIntervalTriggers[intervalTrigger.Key] = intervalTrigger.Value - MAX_TIME;
            } 
            if (overflow) worldTimeUI -= MAX_TIME;

        }
        void RemoveTimeTrigger()
        {
            while (removeQueue.Count > 0)
            {
                var remove = removeQueue.Dequeue();
                
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
        /// </summary>
        /// <param name="key">event key</param>
        /// <param name="interval">action delay</param>
        /// <param name="isLoop"></param>
        /// <param name="action">action</param>
        public bool AddTimeEvent(string key, float interval)
        {
            // If key is already exist, return
            if (_intervalTriggers.ContainsKey(key))
                return false;
            // Add time event
            _intervalTriggers.Add(key, interval+ worldTime);
            return true;
        }
        public bool AddTimeEvent(string key, float interval, Action action)
        {
            // If key is already exist, return
            if (!AddTimeEvent(key, interval)) return false;
            // Add time event
            _intervalActions.Add(key, action);
            return true;
        }
        /// <summary>
        /// Add ui time event
        /// </summary>
        /// <param name="key">event key</param>
        /// <param name="interval">action delay</param>
        /// <param name="isLoop"></param>
        /// <param name="action">action</param>
        public bool AddUITimeEvent(string key, float interval)
        {
            // If key is already exist, return
            if (_uiIntervalTriggers.ContainsKey(key))
                return false;
            // Add time event
            _uiIntervalTriggers.Add(key, interval+ worldTime);
            return true;
        }
        public bool AddUITimeEvent(string key, float interval, Action action)
        {
            // If key is already exist, return
            if (!AddUITimeEvent(key, interval)) return false;
            // Add time event
            _intervalActions.Add(key, action);
            return true;
        }
        /// <summary>
        /// Remove time event
        /// </summary>
        /// <param name="key">event key for remove event</param>
        public bool RemoveTimeEvent(string key)
        {
            if (_intervalActions.ContainsKey(key))
                _intervalActions.Remove(key);
            // If key is exist, remove event
            if (_intervalTriggers.ContainsKey(key))
            {
                _intervalTriggers.Remove(key);
                return true;
            }
            return false;
        }
        /// <summary>
        /// Remove ui time event
        /// </summary>
        /// <param name="key">event key for remove event</param>
        public bool RemoveUITimeEvent(string key)
        {
            if (_intervalActions.ContainsKey(key))
                _intervalActions.Remove(key);
            // If key is exist, remove event
            if (_uiIntervalTriggers.ContainsKey(key))
            {
                _uiIntervalTriggers.Remove(key);
                return true;
            }
            return false;
        }
        /// <summary>
        /// Get time event remain time
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public float GetRemainingTime(string key)
        {
            if (_intervalTriggers.ContainsKey(key))
                return _intervalTriggers[key] - worldTime;
            return -1.0f;
        }
        /// <summary>
        /// Clear all time event
        /// </summary>
        public void ClearTimeEvent()
        {
            _intervalTriggers.Clear();
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