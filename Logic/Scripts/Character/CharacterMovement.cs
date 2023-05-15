using System;
using System.Threading.Tasks;
using JH.Portfolio.Animation;
using JH.Portfolio.Manager;
using Unity.VisualScripting;
using UnityEngine;

namespace JH.Portfolio.Character
{
    [RequireComponent(typeof(CharacterInfo), typeof(Rigidbody))]
    public class CharacterMovement : MonoBehaviour
    {
        System.Threading.CancellationTokenSource _jumpCts;

        [System.Serializable]
        public enum CharacterState
        {
            idle,
            run,
            attack,
            skill,
            dead,
        }

        private CharacterInfo _info;
        private CharacterState _currentCharacterState;
        private Rigidbody _rb;

        public SerializedDictionary<string, SupportAnimation> stateAnimations =
            new SerializedDictionary<string, SupportAnimation>();

        public string currentSupportName;

        private Vector3 _moveDirection = Vector3.zero;
        private Vector3 _input = Vector3.zero;
        private float _accelerationTime = 0;

        private Transform _directionTf;
        private Transform _unitTf;
        private Transform _shadowTf;

        public float jumpHeight = .5f;
        public AnimationCurve jumpCurve;
        public AnimationCurve shadowCurve;

        private int currentDir = 1;
        private int jumpCount = 0;

        public CharacterState CurrentCharacterState
        {
            get => _currentCharacterState;
            set
            {
                if (value == _currentCharacterState) return;
                _currentCharacterState = value;
                CurrentSupportAnimation.PlayAnimation(value.ToString());
            }
        }
        SupportAnimation CurrentSupportAnimation => stateAnimations[currentSupportName];

        void SetTransforms()
        {
            if (!stateAnimations.ContainsKey(currentSupportName)) return;
            _directionTf = stateAnimations[currentSupportName].transform;

            _unitTf = _directionTf.GetChild(0);
            _shadowTf = _directionTf.GetChild(1);
        }

        private void Awake()
        {
            _info = GetComponent<CharacterInfo>();
            _rb = GetComponent<Rigidbody>();
            SetTransforms();
        }
        private void OnEnable()
        {
            GameManager.InputManager.OnMoveInputEvent.OnHeldEvent += InputMove;
            GameManager.InputManager.OnMoveInputEvent.OnReleasedEvent += InputStop;
            GameManager.InputManager.OnJumpInputEvent.OnPressedEvent += JumpInput;
        }
        private void OnDisable()
        {
            _jumpCts?.Cancel();

            if (GameManager.InputManager == null) return;
            GameManager.InputManager.OnMoveInputEvent.OnHeldEvent -= InputMove;
            GameManager.InputManager.OnMoveInputEvent.OnReleasedEvent -= InputStop;
            GameManager.InputManager.OnJumpInputEvent.OnPressedEvent -= JumpInput;
        }

        private void Update()
        {
            Move();
        }

        void Move()
        {
            if (_input != Vector3.zero) // state == run
            {
                CurrentCharacterState = CharacterState.run; // set state

                _accelerationTime =
                    Mathf.Clamp01(_accelerationTime + TimeManager.DeltaTime * _info.Speed);
                var moveDirection = Vector3.Lerp(_moveDirection, _input, _accelerationTime);
                _rb.MovePosition(transform.position +
                                 moveDirection * (_info.MoveSpeed * TimeManager.DeltaTime));

                int nextDir = -_input.x.CompareTo(0);

                if (currentDir != nextDir && nextDir != 0)
                {
                    _directionTf.localScale = new Vector3(nextDir, 1, 1);
                    _accelerationTime = 0.1f;
                    currentDir = nextDir;
                }

                if (_accelerationTime >= 1)
                    _moveDirection = moveDirection;
                
                if (jumpCount == 0)
                    CurrentSupportAnimation.SetAnimation(true, 0, 0.1f, 0, _accelerationTime);
            }
            else if (_accelerationTime > 0) // Stop
            {
                _accelerationTime = Mathf.Clamp01(_accelerationTime - TimeManager.DeltaTime * 30);

                var moveDirection = Vector3.Lerp(Vector3.zero, _moveDirection, _accelerationTime);
                _rb.MovePosition(transform.position +
                                 moveDirection * (_info.MoveSpeed * TimeManager.DeltaTime));
                if (jumpCount == 0)
                    CurrentSupportAnimation.SetAnimation(true, 0, 0.1f, 0, _accelerationTime);
            }
            else // state == idle
            {
                _moveDirection = Vector3.zero;
                CurrentCharacterState = CharacterState.idle;
                CurrentSupportAnimation.SetAnimation(true, 0, 0.1f, 0, 1);
            }
        }
        public void InputMove(Vector3 inputVector) => _input = inputVector;
        void InputStop(Vector3 inputVector) => _input = Vector3.zero;
        public void JumpInput()
        {
            if (jumpCount >= 2) return;
            jumpCount++;
            CurrentSupportAnimation.SetAnimation(true, 0, 0.1f, 0, 0.5f);
            CurrentSupportAnimation.PlayAnimation(CharacterState.run.ToString());
            _jumpCts?.Cancel();
            var jumpCts = new System.Threading.CancellationTokenSource();
            var token = jumpCts.Token;
            token.Register(() => jumpCts.Dispose());
            _jumpCts = jumpCts;
            JumpAsync(token);
        }

        async Task JumpAsync(System.Threading.CancellationToken token)
        {
            try
            {
                await Task.Yield();
                _rb.useGravity = false;
                var startHeight = _unitTf.localPosition.y;
                var height = startHeight;
                
                var doubleHeight = (jumpHeight) * 2 + jumpHeight;
                
                float CalculateT(float height) => (doubleHeight - height) / doubleHeight;
                
                // up
                for (float ut = 0; ut < .5f && !token.IsCancellationRequested; ut += TimeManager.DeltaTime * 2)
                {
                    height = startHeight + jumpCurve.Evaluate(ut) * jumpHeight;
                    var scale = CalculateT(height) * Vector3.one;
                    _unitTf.localPosition = new Vector3(0, height, 0);
                    _shadowTf.localScale = scale;
                    await Task.Yield();
                }

                startHeight = height;
                var d = CalculateT(height);
                // down
                for (float dt = 0.5f; dt < 1 && !token.IsCancellationRequested; 
                     dt += TimeManager.DeltaTime * 2 * d)
                {
                    height = startHeight * jumpCurve.Evaluate(dt);
                    var scale = CalculateT(height);
                    _unitTf.localPosition = new Vector3(0, height, 0);
                    _shadowTf.localScale = scale * Vector3.one;
                    await Task.Yield();
                }

                if (!token.IsCancellationRequested || jumpCount == 0)
                {
                    jumpCount = 0;
                    _rb.useGravity = true;
                    CurrentSupportAnimation.SetAnimation(true, 0, 0.1f, 0, 1);
                    CurrentSupportAnimation.PlayAnimation(CurrentCharacterState.ToString());
                }
                
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"error exception : {e}");
            }
        }
    }
}