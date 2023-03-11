using System;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace JH.Portfolio.Manager
{
    public class TimeManager
    {
        private readonly float MAX_TIME = 1000000f;
        // World time
        private float worldTime = 0;
        // Time scale
        public float TimeScale { get; set; } = 1f;

        // Dictionary for time event
        Dictionary<string, float> _intervalTriggers = new Dictionary<string, float>();
        // Remove queue
        Queue<string> removeQueue = new Queue<string>();
        
        /// <summary>
        /// Update time by deltaTime
        /// </summary>
        /// <param name="deltaTime"></param>
        public void Update(float deltaTime)
        {
            // Set world time
            worldTime += deltaTime * TimeScale;
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
        }

        /// <summary>
        /// Add time event
        /// </summary>
        /// <param name="key">event key</param>
        /// <param name="interval">action delay</param>
        /// <param name="isLoop"></param>
        /// <param name="action">action</param>
        public void AddTimeEvent(string key, float interval)
        {
            // If key is already exist, return
            if (_intervalTriggers.ContainsKey(key))
                return;
            // Add time event
            _intervalTriggers.Add(key, interval+ worldTime);
        }

        /// <summary>
        /// Remove time event
        /// </summary>
        /// <param name="key">event key for remove event</param>
        public void RemoveTimeEvent(string key)
        {
            // If key is exist, remove event
            if (_intervalTriggers.ContainsKey(key))
                _intervalTriggers.Remove(key);
        }

        /// <summary>
        /// Get time event remain time
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public float GetRemainTime(string key)
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
            _intervalTriggers = null;
        }
    }
}