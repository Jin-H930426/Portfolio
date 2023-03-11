using System;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace JH.Portfolio.Manager
{
    public class TimeManager
    {
        // World time
        private float worldTime = 0;
        // Time scale
        public float TimeScale { get; set; } = 1f;

        // IntervalTrigger class
        public class IntervalTrigger
        {
            public bool IsLoop;
            public float Interval;
            public float Timer;
            public float remainTime => Interval - Timer;
            public Action Action;

            /// <summary>
            /// IntervalTrigger constructor
            /// </summary>
            /// <param name="interval">delay Time</param>
            /// <param name="isLoop">loop check</param>
            /// <param name="action">action event</param>
            public IntervalTrigger(float interval, bool isLoop, Action action)
            {
                this.IsLoop = isLoop;
                this.Interval = interval;
                this.Timer = 0f;
                this.Action = action;
            }

            /// <summary>
            /// Set timer and return true if timer is over interval
            /// </summary>
            /// <param name="deltaTime"></param>
            /// <returns></returns>
            public bool Update(float deltaTime)
            {
                Timer += deltaTime;
                if (Timer >= Interval)
                {
                    Action?.Invoke();
                    Timer -= Interval;
                    return true;
                }

                return false;
            }
        }

        // Dictionary for time event
        Dictionary<string, IntervalTrigger> _timeEvent = new Dictionary<string, IntervalTrigger>();
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
            // Update time event
            foreach (var intervalTrigger in _timeEvent)
            {
                if (intervalTrigger.Value.Update(deltaTime * TimeScale) && !intervalTrigger.Value.IsLoop)
                    removeQueue.Enqueue(intervalTrigger.Key);
            }
            // Remove time event
            while (removeQueue.Count > 0)
            {
                _timeEvent.Remove(removeQueue.Dequeue());
            }
        }

        /// <summary>
        /// Add time event
        /// </summary>
        /// <param name="key">event key</param>
        /// <param name="interval">action delay</param>
        /// <param name="isLoop"></param>
        /// <param name="action">action</param>
        public void AddTimeEvent(string key, float interval, bool isLoop, Action action)
        {
            // If key is already exist, return
            if (_timeEvent.ContainsKey(key))
                return;
            // Add time event
            _timeEvent.Add(key, new IntervalTrigger(interval, isLoop, action));
        }

        /// <summary>
        /// Remove time event
        /// </summary>
        /// <param name="key">event key for remove event</param>
        public void RemoveTimeEvent(string key)
        {
            // If key is exist, remove event
            if (_timeEvent.ContainsKey(key))
                _timeEvent.Remove(key);
        }

        /// <summary>
        /// Get time event remain time
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public float GetRemainTime(string key)
        {
            if (_timeEvent.ContainsKey(key))
                return _timeEvent[key].remainTime;
            return -1.0f;
        }
        
        /// <summary>
        /// Clear all time event
        /// </summary>
        public void ClearTimeEvent()
        {
            _timeEvent.Clear();
        }
    }
}