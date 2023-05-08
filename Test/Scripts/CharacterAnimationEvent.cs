using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JH.Test
{
    public class CharacterAnimationEvent : MonoBehaviour
    {
        
        public void OnFootL(int speed)
        {
            // Debug.Log($"OnFootL : {speed}");
        }
        public void OnFootR(int speed)
        {
            // Debug.Log($"OnFootR : {speed}");
        }
        
        public void OnAttack()
        {
            // Debug.Log("OnAttack");
        }
        public void OnAttackEnd()
        {
            // Debug.Log("OnAttackEnd");
        }
        
        public void OnJump(int speed)
        {
            // Debug.Log("OnJump");
        }
        public void OnJumpEnd()
        {
            // Debug.Log("OnJumpEnd");
        }
        
        public void OnSkill(string skillName)
        {
            // Debug.Log("OnSkill");
        }
        
        public void OnDamage(Vector3 normal)
        {
            // Debug.Log("OnDamage");
        }   
        public void OnDamageEnd()
        {
            // Debug.Log("OnDamageEnd");
        }
        
        public void OnDeath()
        {
            // Debug.Log("OnDeath");
        }
    }
}