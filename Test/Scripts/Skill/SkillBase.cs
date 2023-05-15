using JH.Portfolio.Manager;
using UnityEngine;

namespace JH.Test
{
    using Portfolio.Character;
    public abstract class SkillBase : MonoBehaviour
    {
        [SerializeField] SkillData skillData;

        public void CallSkill()
        {
            if (skillData == null)
            {
                return;
            }

            if (TimeManager.AddTimeEvent(skillData.skillName, skillData.skillCoolDown))
            {
                SkillAction();
            }
        }
        
        protected abstract void SkillAction();
    }
}