using UnityEngine;
using UnityEngine.Serialization;

namespace JH.Portfolio.InputSystem
{
    [CreateAssetMenu(fileName = "DesktopKeymap", menuName = "Manager/Input/DesktopKeymap", order = 0)]
    public class DesktopKeymap : Keymap
    {
        public bool useMouse = true;
        /// <summary>
        /// movement input key maps
        /// </summary>
        public MovementInput MoveKey = new()
        {
            forward = KeyCode.W, 
            backward = KeyCode.S, 
            left = KeyCode.A, 
            right = KeyCode.D
        };
        /// <summary>
        /// movement input key maps 2
        /// </summary>
        public MovementInput KeyRotation = new()
        {
            forward = KeyCode.UpArrow, 
            backward = KeyCode.DownArrow, 
            left = KeyCode.LeftArrow, 
            right = KeyCode.RightArrow
        };
        /// <summary>
        /// Mouse axis input names
        /// </summary>
        public AxisInput AxisMouseKey = new()
        {
            axisX = "Mouse X", 
            axisY = "Mouse Y"
        };
        
        /// <summary>
        /// attack input key map
        /// </summary>
        public KeyCode attackKey = KeyCode.Mouse0;
        /// <summary>
        /// reload input key map
        /// </summary>
        public KeyCode reloadKey = KeyCode.Mouse1;
        /// <summary>
        /// jump input key map
        /// </summary>
        public KeyCode jumpKey = KeyCode.Space;
        /// <summary>
        /// crouch input key map
        /// </summary>
        public KeyCode crouchKey = KeyCode.LeftControl;
        /// <summary>
        /// sprint input key map
        /// </summary>
        public KeyCode sprintKey = KeyCode.LeftShift;   
        /// <summary>
        /// hot keys input key maps
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
        
        public override Vector3 GetMovementInput()
        {
            var horizontal = (Input.GetKey(MoveKey.left) ? -1 : 0) + (Input.GetKey(MoveKey.right) ? 1 : 0);
            var vertical = (Input.GetKey(MoveKey.forward) ? 1 : 0) + (Input.GetKey(MoveKey.backward) ? -1 : 0);
            
            return new Vector3(horizontal, 0, vertical);
        }
        public override Vector3 GetRotationInput()
        {
            if (!useMouse)
            {
                var h = (Input.GetKey(KeyRotation.left) ? -1 : 0) + (Input.GetKey(KeyRotation.right) ? 1 : 0);
                var v = (Input.GetKey(KeyRotation.forward) ? 1 : 0) + (Input.GetKey(KeyRotation.backward) ? -1 : 0);
            
                return new Vector3(h,v, 0); 
            }
            else
            {
                var h = Input.GetAxis(AxisMouseKey.axisX);
                var v = Input.GetAxis(AxisMouseKey.axisY);
                
                return new Vector3(h, v, 0);
            }
        }
        public override bool GetAttackInput()
        {
            return Input.GetKey(attackKey);
        }
        public override bool GetReloadInput()
        {
            return Input.GetKey(reloadKey);
        }
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
        
        public override ulong GetHotKeyInput()
        {
            ulong inputValue = 0;
            foreach (var hotKey in hotKeys)
            {
                if (Input.GetKey(hotKey))
                    inputValue |= 1;
                inputValue <<= 1;
            }
            return inputValue >> 1;
        }
        public override int GetHotKeyCount()
        {
            return hotKeys.Length;
        }
    }
}