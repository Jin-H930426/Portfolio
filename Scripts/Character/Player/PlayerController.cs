using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JH.Portfolio.Character
{
    using Manager;
    [RequireComponent(typeof(Animator), typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        // Reference to Animator
        private Animator _animator;
        // Reference to Rigidbody
        private Rigidbody _rigidbody;

        [field: SerializeField] public float moveSpeed { get; private set; } = 5f;
        [field: SerializeField] public float rotateSpeed { get; private set; } = 360f;
        [field: SerializeField] public float jumpForce { get; private set; } = 5f;
        [field: SerializeField] public float gravity { get; private set; } = 9.8f;
        
        // State of player's run
        private bool _isMove = false;
        // State of player's defense
        private bool _isDefense = false;
        // State of player's sprint
        private bool _isSprint = false;
        // State of player's jump
        private bool _isJump = false;
        
        // Valuable of player's Idle state
        private float _idleVal = 0f;
        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _rigidbody = GetComponent<Rigidbody>();
        }

        /// <summary>
        /// Initialize on second enable
        /// Because GameManager is initialized after PlayerController
        /// </summary>
        private void OnEnable()
        {
            // Check if GameManager is initialized
            if (GameManager.InputManager == null) return;
            // Initialize
            Initialize();
        }

        /// <summary>
        /// Initialize on first enable
        /// Because GameManager is initialized after PlayerController
        /// </summary>
        private void Start()
        {
            // Initialize
            Initialize();
        }

        /// <summary>
        /// Initialize
        /// </summary>
        private void Initialize()
        {
            // Initialize input event
            InitializationEvents();
            // Initialize animator
            InitializationAnimator();
        }

        /// <summary>
        /// Initialize input event
        /// </summary>
        private void InitializationEvents()
        {
            GameManager.InputManager.OnRotNMoveInputEvent.OnHeldEvent += RotateNMovement;
            GameManager.InputManager.OnRotNMoveInputEvent.OnReleasedEvent += Stop;

            GameManager.InputManager.OnAttackInputEvent.OnPressedEvent += Attact;
            GameManager.InputManager.OnReloadInputEvent.OnPressedEvent += Defense;
            GameManager.InputManager.OnReloadInputEvent.OnReleasedEvent += ReleaseDefense;

            GameManager.InputManager.OnSprintInputEvent.OnPressedEvent += Sprint;
            GameManager.InputManager.OnSprintInputEvent.OnReleasedEvent += ReleaseSprint;
        }

        /// <summary>
        /// Initialize animator
        /// </summary>
        private void InitializationAnimator()
        {
            _animator.SetFloat("Speed_f", 0f);
            _animator.SetBool("Defense_b", false);
            _isSprint = false;
            _isDefense = false;
        }

        void RotateNMovement(Vector3 inputMove, Vector3 inputRotate)
        {
            CharacterMovement(inputMove);
        }

        void CharacterMovement(Vector3 inputVector)
        {
            if (_isJump) return;
            // Get horizontal and vertical input
            float vertical = inputVector.z * .5f;
            float horizontal = inputVector.x * .5f;
            
            // Check sprint
            if (_isSprint)
            {
                horizontal *= 2f;
                vertical *= 2f;
            }
            
            // Calculate rotation and movement vector
            var rotVector = new Vector3(0, horizontal, 0) * (rotateSpeed * TimeManager.DeltaTime);
            var moveVector = transform.TransformDirection(new Vector3(0, 0, vertical)) * (moveSpeed * TimeManager.DeltaTime);
            // Rotate
            Quaternion rotation = Quaternion.Euler(rotVector) * transform.rotation;
            Vector3 position = transform.position + moveVector;
            
            // Apply
            transform.SetPositionAndRotation(position, rotation);
            // Set animation
            // move Statue Check
            if (inputVector.magnitude > 0.2f && !_isMove)
            {
                _isMove = true;
                _animator.SetBool("Move_b", true);
            }

            
            _animator.SetFloat("Horizontal_f", horizontal);
            _animator.SetFloat("Vertical_f", vertical);
        }
        
        
        void Stop(Vector3 movem, Vector3 rot)
        {
            // Set animation
            _isMove = false;
            _animator.SetBool("Move_b", false);
        }

        void Attact()
        {
            _animator.SetTrigger("Attack_t");
        }

        void Defense()
        {
            _isDefense = true;
            _animator.ResetTrigger("Attack_t");
            _animator.SetBool("Defense_b", true);
        }

        void ReleaseDefense()
        {
            _isDefense = false;
            _animator.SetBool("Defense_b", false);
        }

        void Sprint()
        {
            _isSprint = true;
        }

        void ReleaseSprint()
        {
            _isSprint = false;
        }

        /// <summary>
        /// Clear input event when disable
        /// </summary>
        private void OnDisable()
        {
            ClearInputEvent();
        }

        /// <summary>
        /// Clear input event
        /// </summary>
        private void ClearInputEvent()
        {
            // Check if GameManager is initialized
            if (GameManager.InputManager == null) return;

            GameManager.InputManager.OnRotNMoveInputEvent.OnHeldEvent -= RotateNMovement;
            GameManager.InputManager.OnRotNMoveInputEvent.OnReleasedEvent -= Stop;

            GameManager.InputManager.OnAttackInputEvent.OnPressedEvent -= Attact;
            GameManager.InputManager.OnReloadInputEvent.OnPressedEvent -= Defense;
            GameManager.InputManager.OnReloadInputEvent.OnReleasedEvent -= ReleaseDefense;

            GameManager.InputManager.OnSprintInputEvent.OnPressedEvent -= Sprint;
            GameManager.InputManager.OnSprintInputEvent.OnReleasedEvent -= ReleaseSprint;
        }
    }
}