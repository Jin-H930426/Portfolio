using UnityEngine;

namespace JH.Portfolio.InputSystem
{
    public abstract class GetInput
    {
        // 이동 및 회전 입력 처리
        /// <summary>
        /// Get movement input vector
        /// </summary>
        /// <returns>input value</returns>
        public abstract Vector3 GetMovementInput();
        /// <summary>
        /// Get rotation input vector
        /// </summary>
        /// <returns>input value</returns>
        public abstract Vector3 GetRotationInput();
        
        // 공격 및 재장전 입력 처리
        /// <summary>
        /// Get attack input
        /// </summary>
        /// <returns>input value</returns>
        public abstract bool GetAttackInput();
        /// <summary>
        /// Get reload input
        /// </summary>
        /// <returns></returns>
        public abstract bool GetReloadInput();
        // 점프, 앉기, 달리기 입력 처리
        /// <summary>
        /// Get jump input
        /// </summary>
        /// <returns>input value</returns>
        public abstract bool GetJumpInput();
        /// <summary>
        /// Get crouch input
        /// </summary>
        /// <returns>input value</returns>
        public abstract bool GetCrouchInput();
        /// <summary>
        /// Get sprint input
        /// </summary>
        /// <returns>input value</returns>
        public abstract bool GetSprintInput();
        /// <summary>
        /// Get pause key inputs
        /// </summary>
        /// <returns></returns>
        public abstract ulong GetHotKeyInput();
        public abstract int GetHotKeyCount();
    }
}