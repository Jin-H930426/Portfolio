using System.Collections;
using System.Collections.Generic;
using JH.Portfolio.Map;
using UnityEngine;
using UnityEngine.Serialization;

namespace JH.Portfolio.Character
{
    public class SkillData : ScriptableObject, IHeapItem<SkillData>
    {
        public int skillID = 0;
        public Sprite skillIcon;
        public string skillName = "NewSkill";
        public string animationName = "";
        [FormerlySerializedAs("damageType")] public SkillType skillType;
        [FormerlySerializedAs("andItemRange")] public SkillRange range;
        public int costHp;
        public int costMp;
        
        public int physicDamage;
        public int masicDamage;

        public int buffStr;
        public int buffDef;
        public int buffSpe;
        public int buffInt;
        public int buffLuc;

        public int skillDuration;
        public int skillCoolDown;
        public string description;
        public int skillPriority;
        public int CompareTo(SkillData other)
        {
            return -skillPriority.CompareTo(other.skillPriority);
        }

        public int HeapIndex { get; set; }
    }

    public enum SkillRange
    {
        Self = 1,
        TeamUnit = 2,
        TeamAll = 4,
        EnemyUnit = 8,
        EnemyAll = 16,
    }
    public enum SkillType
    {
        Physic = 1,
        Magic = 2,
        Heal = 4,
        Buff = 8,
        Debuff =16,
    }
}