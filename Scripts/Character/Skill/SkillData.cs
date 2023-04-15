using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JH.Portfolio.Character
{
    public class SkillData : ScriptableObject
    {
        public string skillName = "NewSkill";
        public Texture2D skillIcon;
        public int skillDamage;
        public float skillCoolDown;
        public string description;
    }
}