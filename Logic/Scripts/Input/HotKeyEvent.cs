using System;
using System.Collections;
using System.Collections.Generic;
using JH.Portfolio.Manager;
using UnityEngine;
using UnityEngine.Events;

namespace JH.Portfolio.InputSystem
{
    public class HotKeyEvent : MonoBehaviour
    {
        public int keyIndex = 0;
        
        public UnityEvent onPressHotKey = new UnityEvent();
        public UnityEvent onHeldHotKey = new UnityEvent();
        public UnityEvent onReleasHotKey= new UnityEvent();

        private void OnEnable()
        {
            if (GameManager.InputManager == null)
                return;
            GameManager.InputManager.OnHotKeyInputEvents[keyIndex].OnPressedEvent += onPressHotKey.Invoke;
            GameManager.InputManager.OnHotKeyInputEvents[keyIndex].OnReleasedEvent += onReleasHotKey.Invoke;
        }
       
        private void OnDisable()
        {
            if (GameManager.InputManager is not InputManager manager) return;
            manager.OnHotKeyInputEvents[keyIndex].OnPressedEvent -= onPressHotKey.Invoke;
            manager.OnHotKeyInputEvents[keyIndex].OnHeldEvent -= onHeldHotKey.Invoke;
            manager.OnHotKeyInputEvents[keyIndex].OnReleasedEvent -= onReleasHotKey.Invoke;
        }
    }
}