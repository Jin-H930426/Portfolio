using System;
using System.Collections;
using System.Collections.Generic;
using JH.Portfolio.Manager;
using UnityEngine;

namespace JH.Portfolio.Character
{
    public class PlayerController : MonoBehaviour
    {
        // Reference to Animator
        [SerializeField] private Animator _animator;

        [field: SerializeField] public float moveSpeed { get; private set; } = 5f;
        [field: SerializeField] public float rotateSpeed { get; private set; } = 360f;
        [field: SerializeField] public float jumpForce { get; private set; } = 5f;
        [field: SerializeField] public float gravity { get; private set; } = 9.8f;

        // State of player's defense
        private bool _isDefense = false;

        // State of player's sprint
        private bool _isSprint = false;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
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
            _animator.SetBool("Sprint_b", false);
            _isSprint = false;
            _isDefense = false;
        }

        void RotateNMovement(Vector3 move, Vector3 rot)
        {
            // Calculate speed
            var speed_f = move.normalized.magnitude;
            // Check spring and defense
            if (move.z > .9f && _isSprint && !_isDefense)
                speed_f = 1f;
            else if (speed_f > .5f)
                speed_f = .5f;
            // Calculate rotation and movement vector
            var rotVector = new Vector3(0, -rot.x, 0) * (rotateSpeed * TimeManager.DeltaTime);
            var moveVector = transform.TransformDirection(move) * (moveSpeed * TimeManager.DeltaTime * speed_f);
            // Rotate
            Quaternion rotation = Quaternion.Euler(rotVector) * transform.rotation;
            // Move
            Vector3 movement = transform.position + moveVector;
            // Apply
            transform.SetPositionAndRotation(movement, rotation);

            // Set animation
            _animator.SetFloat("Speed_f", speed_f);
        }

        void Stop(Vector3 movem, Vector3 rot)
        {
            // Set animation
            _animator.SetFloat("Speed_f", 0f);
        }

        void Attact()
        {
            _animator.SetTrigger("Attack_t");
        }

        void Defense()
        {
            _isDefense = true;
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