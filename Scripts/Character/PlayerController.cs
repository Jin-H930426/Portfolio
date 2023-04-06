using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JH.Portfolio.Controller
{
    using Manager;
    [RequireComponent(typeof(Animator), typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        // Reference to Animator
        private Animator _animator;
        // Reference to Rigidbody
        private Rigidbody _rigidbody;

        [field: SerializeField] public float MoveSpeed { get; private set; } = 5f;
        [field: SerializeField] public float RotateSpeed { get; private set; } = 360f;
        [field: SerializeField] public float JumpForce { get; private set; } = 5f;
        
        // State of player's run
        private bool _isMove = false;
        // State of player's defense
        private bool _isDefense = false;
        // State of player's sprint
        private bool _isSprint = false;
        // State of player's jump
        private int _jumpCount = 0;
        [SerializeField] public int maxJumpCount = 2;
        
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
            
            GameManager.InputManager.OnJumpInputEvent.OnReleasedEvent += Jump;
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
            
            GameManager.InputManager.OnJumpInputEvent.OnPressedEvent -= Jump;
        }
        /// <summary>
        /// Event for character movement by input manager
        /// </summary>
        /// <param name="inputMove"></param>
        /// <param name="inputRotate"></param>
        void RotateNMovement(Vector3 inputMove, Vector3 inputRotate)
        {
            CharacterMovement(inputMove);
        }
        
        /// <summary>
        /// Function for character movement
        /// </summary>
        /// <param name="inputVector"></param>
        void CharacterMovement(Vector3 inputVector)
        {
            // Get horizontal and vertical input
            float vertical = inputVector.z;
            float horizontal = inputVector.x;
            
            // Check sprint
            if (!_isSprint)
            {
                horizontal *= .5f;
                vertical *= .5f;
            }
            
            // Calculate rotation and movement vector
            var rotVector = new Vector3(0, horizontal, 0) * (RotateSpeed * TimeManager.DeltaTime);
            var moveVector = transform.TransformDirection(new Vector3(0, 0, vertical)) * (MoveSpeed * TimeManager.DeltaTime);
            
            // Calculate rotation
            // If jumpCount over 0, set Player's rotation to zero
            if (_jumpCount > 0) rotVector = Vector3.zero;
            Quaternion rotation = Quaternion.Euler(rotVector) * transform.rotation;
            // Calculate position
            Vector3 position = transform.position + moveVector;
            // Apply rotation and position
            transform.SetPositionAndRotation(position, rotation);
            
            // Set animation
            // move Statue Check for move animation
            if (inputVector.magnitude > 0.2f && !_isMove)
            {
                // Set movement parameter as true for move animation
                _isMove = true;
                _animator.SetBool("Move_b", true);
            }
            // Set movement parameter as vertical and horizontal input for blend tree
            _animator.SetFloat("Horizontal_f", horizontal);
            _animator.SetFloat("Vertical_f", vertical);
        }
        /// <summary>
        /// Stop character movement
        /// </summary>
        /// <param name="movem"></param>
        /// <param name="rot"></param>
        void Stop(Vector3 movem, Vector3 rot)
        {
            // Set animation
            _isMove = false;
            _animator.SetBool("Move_b", false);
        }

        /// <summary>
        /// Play attack animation
        /// </summary>
        void Attact()
        {
            _animator.SetTrigger("Attack_t");
        }
        /// <summary>
        /// Set defense state
        /// </summary>
        void Defense()
        {
            _isDefense = true;
            _animator.ResetTrigger("Attack_t");
            _animator.SetBool("Defense_b", true);
        }
        /// <summary>
        /// Reset defense state
        /// </summary>
        void ReleaseDefense()
        {
            _isDefense = false;
            _animator.SetBool("Defense_b", false);
        }
        /// <summary>
        /// Set sprint state
        /// </summary>
        void Sprint()
        {
            if (_jumpCount > 0)
            {
                return;
            }
            _isSprint = true;
        }
        /// <summary>
        /// Reset sprint state
        /// </summary>
        void ReleaseSprint()
        {
            _isSprint = false;
        }
        /// <summary>
        /// Jump character
        /// </summary>
        void Jump()
        {
            if (_jumpCount >= maxJumpCount) return;
            _jumpCount++;
            _rigidbody.AddForce(Vector3.up * JumpForce, ForceMode.Impulse);
            _animator.SetTrigger("Jump_t");
        }
        
        /// <summary>
        /// Check collision
        /// </summary>
        /// <param name="collision"></param>
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Ground"))
            {
                _jumpCount = 0;
            }
        }
    }
}