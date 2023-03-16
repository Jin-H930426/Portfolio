using UnityEngine;

namespace JH.Portfolio.InputSystem
{
    public class DesktopGetInput : GetInput
    {
        /// <summary>
        /// 이동처리하는 키맵
        /// </summary>
        public KeyCode[] moveKeys = new[] { KeyCode.W, KeyCode.S, KeyCode.A, KeyCode.D };
        /// <summary>
        /// 카메라 회전을 사용하는지 여부
        /// </summary>
        public bool UseMouse = true;
        /// <summary>
        /// 공격을 처리하는 키맵
        /// </summary>
        public KeyCode attackKey = KeyCode.Mouse0;
        /// <summary>
        /// reload를 처리하는 키맵
        /// </summary>
        public KeyCode reloadKey = KeyCode.Mouse1;
        /// <summary>
        /// 점프를 처리하는 키맵
        /// </summary>
        public KeyCode jumpKey = KeyCode.Space;
        /// <summary>
        /// 앉기를 처리하는 키맵
        /// </summary>
        public KeyCode crouchKey = KeyCode.LeftControl;
        /// <summary>
        /// 달리기를 처리하는 키맵
        /// </summary>
        public KeyCode sprintKey = KeyCode.LeftShift;   
        /// <summary>
        /// 단축키 키맵 ulong은 64비트로 최대 64개의 키를 처리할 수 있다.
        /// </summary>
        public KeyCode[] hotKeys = new[] 
            { 
                KeyCode.Alpha1, 
                KeyCode.Alpha2, 
                KeyCode.Alpha3, 
                KeyCode.Alpha4, 
                KeyCode.Alpha5, 
                KeyCode.Alpha6, 
                KeyCode.Alpha7, 
                KeyCode.Alpha8, 
                KeyCode.Alpha9 
            };
        
        // 이동 및 회전 입력 처리
        public override Vector3 GetMovementInput()
        {
            var horizontal = (Input.GetKey(moveKeys[2]) ? -1 : 0) + (Input.GetKey(moveKeys[3]) ? 1 : 0);
            var vertical = (Input.GetKey(moveKeys[0]) ? 1 : 0) + (Input.GetKey(moveKeys[1]) ? -1 : 0);
            
            return new Vector3(horizontal, 0, vertical);
        }
        public override Vector3 GetRotationInput()
        {
            if(!UseMouse) return  Vector3.zero;
            
            var horizontal = Input.GetAxis("Mouse X");
            var vertical = Input.GetAxis("Mouse Y");
            
            return new Vector3(horizontal, vertical, 0);
        }
        // 공격 및 재장전 입력 처리
        public override bool GetAttackInput()
        {
            return Input.GetKey(attackKey);
        }
        public override bool GetReloadInput()
        {
            return Input.GetKey(reloadKey);
        }
        // 점프, 앉기, 달리기 입력 처리
        public override bool GetJumpInput()
        {
            return Input.GetKey(jumpKey);
        }
        public override bool GetCrouchInput()
        {
            return Input.GetKey(crouchKey);
        }
        public override bool GetSprintInput()
        {
            return Input.GetKey(sprintKey);
        }
        
        // 단축키 입력 처리
        public override ulong GetHotKeyInput()
        {
            ulong inputValue = 0;
            foreach (var hotKey in hotKeys)
            {
                if (Input.GetKey(hotKey))
                    inputValue |= 1;
                inputValue <<= 1;
            }
            return inputValue;
        }
        // 단축키 수 반환
        public override int GetHotKeyCount()
        {
            return hotKeys.Length;
        }
    }
}