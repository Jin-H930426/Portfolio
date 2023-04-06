using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace JH.Portfolio.Manager
{
    [System.Serializable]
    public class TimeManager
    {
        private readonly float MAX_TIME = 1000000f;
        
        public static float DeltaTime { get; private set; } = 0f;
        // World time
        [field: SerializeField] public float worldTime { get; private set; } = 0;

        // Time scale
        [field: SerializeField] public float TimeScale { get; set; } = 1f;

        // Dictionary for time event
        private Dictionary<string, float> _intervalTriggers { get; set; } = new Dictionary<string, float>();

        // Remove queue
        private Queue<string> removeQueue { get; set; } = new Queue<string>();

        /// <summary>
        /// Update time by deltaTime
        /// </summary>
        /// <param name="deltaTime"></param>
        public void Update(float deltaTime)
        {
            UnityEngine.Profiling.Profiler.BeginSample("TimeManager.Update");
            // Set world time
            DeltaTime = deltaTime * TimeScale;
            worldTime += DeltaTime;
            bool overflow = worldTime > MAX_TIME;
            // Update time event
            foreach (var intervalTrigger in _intervalTriggers)
            {
                if (intervalTrigger.Value < worldTime)
                    removeQueue.Enqueue(intervalTrigger.Key);
                if (overflow) _intervalTriggers[intervalTrigger.Key] = intervalTrigger.Value - MAX_TIME;
            }
            // Remove time event
            while (removeQueue.Count > 0)
            {
                _intervalTriggers.Remove(removeQueue.Dequeue());
            }
            if (overflow) worldTime -= MAX_TIME;
            UnityEngine.Profiling.Profiler.EndSample();
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
        /// <summary>
        /// Remove time event
        /// </summary>
        /// <param name="key">event key for remove event</param>
        public bool RemoveTimeEvent(string key)
        {
            // If key is exist, remove event
            if (_intervalTriggers.ContainsKey(key))
            {
                _intervalTriggers.Remove(key);
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