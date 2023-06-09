﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace JH.Portfolio.Manager
{
    using InputSystem;
    [Serializable]
    public class InputManager
    {
        #region define
        // Define input type
        [System.Serializable] public enum InputType
        {
            None,
            Keyboard,
            Mobile,
            Joystick
        }
        // Define input event handler
        public class InputEvent
        {
            public ulong BitMask = 1;
            private bool _currentPressState = false;
            
            private Action _onPressedEvent;
            private Action _onHeldEvent;
            private Action _onReleasedEvent;
            public event Action OnPressedEvent
            {
                add => _onPressedEvent += value;
                remove => _onPressedEvent -= value;
            }
            public event Action OnHeldEvent 
            { 
                add => _onHeldEvent += value;
                remove => _onHeldEvent -= value;
            }
            public event Action OnReleasedEvent
            {
                add => _onReleasedEvent += value;
                remove => _onReleasedEvent -= value;
            }

            public void InvokeInputEvent(ulong inputBit)
            {
                var pressState = (inputBit & BitMask) != 0;
                InvokeInputEvent(pressState);
            }
            public void InvokeInputEvent(bool pressState)
            {
                // pressed check
                if (pressState && !_currentPressState)
                {
                    _onPressedEvent?.Invoke();
                }
                // held check
                if (pressState && _currentPressState)
                {
                    _onHeldEvent?.Invoke();
                }
                // released check
                if (!pressState && _currentPressState)
                {
                    _onReleasedEvent?.Invoke();
                }
                
                // update current press state
                _currentPressState = pressState;
            }
            public void Clear()
            {
                _onPressedEvent = null;
                _onHeldEvent = null;
                _onReleasedEvent = null;
                
                _currentPressState = false;
            }
        }
        public class InputEvent<T>
        {
            public ulong BitMask = 1;
            private bool _currentPressState = false;
            
            private Action<T> _onPressedEvent;
            private Action<T> _onHeldEvent;
            private Action<T> _onReleasedEvent;
            
            public event Action<T> OnPressedEvent
            {
                add => _onPressedEvent += value;
                remove => _onPressedEvent -= value;
            }
            public event Action<T> OnHeldEvent
            {
                add => _onHeldEvent += value;
                remove => _onHeldEvent -= value;
            }
            public event Action<T> OnReleasedEvent
            {
                add => _onReleasedEvent += value;
                remove => _onReleasedEvent -= value;
            }
            
            public void InvokeInputEvent(ulong bit, T value)
            {
                var pressState = (bit & BitMask) != 0;
                InvokeInputEvent(pressState, value);
            }
            public void InvokeInputEvent(bool pressState, T value)
            {
                // pressed check
                if (pressState && !_currentPressState)
                {
                    _onPressedEvent?.Invoke(value);
                }
                // held check
                if (pressState && _currentPressState)
                {
                    _onHeldEvent?.Invoke(value);
                }
                // released check
                if (!pressState && _currentPressState)
                {
                    _onReleasedEvent?.Invoke(value);
                }
                
                // update current press state
                _currentPressState = pressState;
            }
            
            public void Clear()
            {
                _onPressedEvent = null;
                _onHeldEvent = null;
                _onReleasedEvent = null;
                
                _currentPressState = false;
            }
        }
        public class InputEvent<T1, T2>
        {
            public ulong BitMask = 1;
            private bool _currentPressState = false;
            private Action<T1, T2> _onPressedEvent;
            private Action<T1, T2> _onHeldEvent;
            private Action<T1, T2> _onReleasedEvent;
            
            public event Action<T1, T2> OnPressedEvent
            {
                add => _onPressedEvent += value;
                remove => _onPressedEvent -= value;
            }
            public event Action<T1, T2> OnHeldEvent
            {
                add => _onHeldEvent += value;
                remove => _onHeldEvent -= value;
            }
            public event Action<T1, T2> OnReleasedEvent
            {
                add => _onReleasedEvent += value;
                remove => _onReleasedEvent -= value;
            }
            
            public void InvokeInputEvent(ulong bit, T1 value1, T2 value2)
            {
                var pressState = (bit & BitMask) != 0;
                InvokeInputEvent(pressState, value1, value2);
            }
            public void InvokeInputEvent(bool pressState, T1 value1, T2 value2)
            {
                // pressed check
                if (pressState && !_currentPressState)
                {
                    _onPressedEvent?.Invoke(value1, value2);
                }
                // held check
                else if (pressState && _currentPressState)
                {
                    _onHeldEvent?.Invoke(value1, value2);
                }
                // released check
                else if (!pressState && _currentPressState)
                {
                    _onReleasedEvent?.Invoke(value1, value2);
                }
                
                // update current press state
                _currentPressState = pressState;
            }
            
            public void Clear()
            {
                _onPressedEvent = null;
                _onHeldEvent = null;
                _onReleasedEvent = null;
                
                _currentPressState = false;
            }
        }
        #endregion
        // GetInput instance
        [ReadOnlyProperty, SerializeField] private Keymap _input;
        // Input type by platform
        [ReadOnlyProperty, SerializeField] private InputType currentInputType = InputType.None;
         
        // Event
        // movement key event
        public InputEvent<Vector3> OnMoveInputEvent { get; set; } = new InputEvent<Vector3>();
        public InputEvent<Vector3> OnRotateInputEvent { get; set; } = new InputEvent<Vector3>();
        // attack key event
        public InputEvent OnAttackInputEvent { get; set; } = new InputEvent();
        // reload key event
        public InputEvent OnReloadInputEvent { get; set; } = new InputEvent();
        // jump key event
        public InputEvent OnJumpInputEvent { get; set; } = new InputEvent();
        // crouch key event
        public InputEvent OnCrouchInputEvent { get; set; } = new InputEvent();
        // sprint key event
        public InputEvent OnSprintInputEvent { get; set; } = new InputEvent();
        // hot key event
        public InputEvent[] OnHotKeyInputEvents { get; set; }
        
        /// <summary>
        /// Constructor of InputManager
        /// </summary>
        /// <param name="inputType"></param>
        public InputManager(InputType inputType)
        {
            SetGetInput(inputType);
        }
        /// <summary>
        /// Initialize input manager
        /// Set this by input type
        /// </summary>
        /// <param name="inputType"></param>
        public void SetGetInput(InputType inputType)
        {
            if(_input is not null) _input = null;
            if (currentInputType == inputType) return;
            currentInputType = inputType;
            // Get Input
            switch (inputType)
            {
                case InputType.Keyboard:
                    _input = GameManager.ResourceManager.LoadOnResource<DesktopKeymap>("Keymap/DesktopKeymap");
                    break;
                case InputType.Mobile:
                    _input = GameManager.ResourceManager.LoadOnResource<DesktopKeymap>("Keymap/MobileKeymap");
                    break;
                default: return;
            }
            
            // initialize hot key events
            OnHotKeyInputEvents = new InputEvent[_input.GetHotKeyCount()];   
            ulong bitMask = (ulong)1 << OnHotKeyInputEvents.Length;
            for (int i = 0; i < OnHotKeyInputEvents.Length; i++)
            {
                bitMask = bitMask >> 1;
                OnHotKeyInputEvents[i] = new InputEvent() { BitMask =  bitMask };
            }
        }
        /// <summary>
        /// Game update loop
        /// </summary>
        /// <param name="deltaTime"></param>
        public void Update(float deltaTime)
        {
            // 회전 및 이동 이벤트 처리
            var moveInput = _input.GetMovementInput();
            var rotationInput = _input.GetRotationInput();
            
            OnMoveInputEvent.InvokeInputEvent(moveInput.sqrMagnitude != 0, moveInput);
            OnRotateInputEvent.InvokeInputEvent(rotationInput.sqrMagnitude != 0, rotationInput);
            // 공격 및 재장전 이벤트 처리
            OnAttackInputEvent.InvokeInputEvent(_input.GetAttackInput());
            OnReloadInputEvent.InvokeInputEvent(_input.GetReloadInput());
            // 점프, 앉기, 달리기 이벤트 처리
            OnJumpInputEvent.InvokeInputEvent(_input.GetJumpInput());
            OnCrouchInputEvent.InvokeInputEvent(_input.GetCrouchInput());
            OnSprintInputEvent.InvokeInputEvent(_input.GetSprintInput());
            // 단축키 이벤트 처리
            int hotkeycount = _input.GetHotKeyCount();
            ulong hotkeyinput = _input.GetHotKeyInput();
            for (int i = 0; i < hotkeycount; i++)
            {
                OnHotKeyInputEvents[i].InvokeInputEvent(hotkeyinput);
            }
        }
        
        /// <summary>
        /// Clear input events
        /// </summary>
        public void ClearInputEvent()
        {
            OnMoveInputEvent.Clear();
            OnRotateInputEvent.Clear();
            OnAttackInputEvent.Clear();
            OnReloadInputEvent.Clear();
            OnJumpInputEvent.Clear();
            OnCrouchInputEvent.Clear();
            OnSprintInputEvent.Clear();
            
            for (int i = 0; i < OnHotKeyInputEvents.Length; i++)
            {
                OnHotKeyInputEvents[i].Clear();
            }
        }
        /// <summary>
        /// Destroy input manager
        /// </summary>
        public void Destroy()
        {
            ClearInputEvent();
            OnMoveInputEvent = null;
            OnRotateInputEvent = null;
            OnAttackInputEvent = null;
            OnReloadInputEvent = null;
            OnJumpInputEvent = null;
            OnCrouchInputEvent = null;
            OnSprintInputEvent = null;
            
            for (int i = 0; i < OnHotKeyInputEvents.Length; i++)
            {
                OnHotKeyInputEvents[i] = null;
            }
            
            OnHotKeyInputEvents = null;
            _input = null;
        }
    }
}