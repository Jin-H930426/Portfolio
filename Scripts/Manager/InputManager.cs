using System;
using UnityEngine;

namespace JH.Portfolio.Manager
{
    using InputSystem;
    public class InputManager
    {
        #region define
        // Define input type
        [System.Serializable] public enum InputType
        {
            Keyboard,
            Mobile,
            Joystick
        }
        // Define input event handler
        public class InputEvent
        {
            private bool _currentPressState = false;
            private Action _onPressedEvent;  
            public event Action OnPressedEvent
            {
                add => _onPressedEvent += value;
                remove => _onPressedEvent -= value;
            }
            private Action _onHeldEvent;
            public event Action OnHeldEvent
            {
                add => _onHeldEvent += value;
                remove => _onHeldEvent -= value;
            }
            private Action _onReleasedEvent;
            public event Action OnReleasedEvent
            {
                add => _onReleasedEvent += value;
                remove => _onReleasedEvent -= value;
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
            private bool _currentPressState = false;
            private Action<T> _onPressedEvent;  
            public event Action<T> OnPressedEvent
            {
                add => _onPressedEvent += value;
                remove => _onPressedEvent -= value;
            }
            private Action<T> _onHeldEvent;
            public event Action<T> OnHeldEvent
            {
                add => _onHeldEvent += value;
                remove => _onHeldEvent -= value;
            }
            private Action<T> _onReleasedEvent;
            public event Action<T> OnReleasedEvent
            {
                add => _onReleasedEvent += value;
                remove => _onReleasedEvent -= value;
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
            private bool _currentPressState = false;
            private Action<T1, T2> _onPressedEvent;  
            public event Action<T1, T2> OnPressedEvent
            {
                add => _onPressedEvent += value;
                remove => _onPressedEvent -= value;
            }
            private Action<T1, T2> _onHeldEvent;
            public event Action<T1, T2> OnHeldEvent
            {
                add => _onHeldEvent += value;
                remove => _onHeldEvent -= value;
            }
            private Action<T1, T2> _onReleasedEvent;
            public event Action<T1, T2> OnReleasedEvent
            {
                add => _onReleasedEvent += value;
                remove => _onReleasedEvent -= value;
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
        private GetInput _input;
        // Input type by platform
        [ReadOnly, SerializeField] private string _inputType;
         
        // Event
        // movement key event
        public InputEvent<Vector3, Vector3> OnRotNMoveInputEvent { get; set; } = new InputEvent<Vector3, Vector3>();
        // attack key event
        public InputEvent OnAttackInputEvent { get; set; } = new InputEvent();
        // reload key evnt
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
            
            switch (inputType)
            {
                case InputType.Keyboard:
                    _input = new DesktopGetInput();
                    _inputType = "Desktop";
                    break;
                case InputType.Mobile:
                    _inputType = "Mobile";
                    Debug.Log("Mobile input is not supported yet");
                    return;
                default: return;
            }
            
            // initialize hot key events
            OnHotKeyInputEvents = new InputEvent[_input.GetHotKeyCount()];            
            for (int i = 0; i < OnHotKeyInputEvents.Length; i++)
            {
                OnHotKeyInputEvents[i] = new InputEvent();
            }
        }
        /// <summary>
        /// Game update loop
        /// </summary>
        /// <param name="deltaTime"></param>
        public void Update(float deltaTime)
        {
            // 회전 및 이동 이벤트 처리
            var move = _input.GetMovementInput();
            var rot = _input.GetRotationInput();
            bool isMove = move.magnitude + rot.magnitude != 0;
            OnRotNMoveInputEvent?.InvokeInputEvent(isMove, move, rot);
            // 공격 및 재장전 이벤트 처리
            OnAttackInputEvent?.InvokeInputEvent(_input.GetAttackInput());
            OnReloadInputEvent?.InvokeInputEvent(_input.GetReloadInput());
            // 점프, 앉기, 달리기 이벤트 처리
            OnJumpInputEvent?.InvokeInputEvent(_input.GetJumpInput());
            OnCrouchInputEvent?.InvokeInputEvent(_input.GetCrouchInput());
            OnSprintInputEvent?.InvokeInputEvent(_input.GetSprintInput());
            // 단축키 이벤트 처리
            int hotkeycount = _input.GetHotKeyCount();
            ulong hotkeyinput = _input.GetHotKeyInput();
            for (int i = 0; i < hotkeycount; i++)
            {
                OnHotKeyInputEvents[i]?.InvokeInputEvent((hotkeyinput & ((ulong)1 << i)) != 0);
            }
        }
        
        /// <summary>
        /// Clear input events
        /// </summary>
        public void ClearInputEvent()
        {
            OnRotNMoveInputEvent.Clear();
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
            OnRotNMoveInputEvent = null;
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