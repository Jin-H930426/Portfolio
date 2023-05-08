using System;

namespace JH.Portfolio.Character
{
    [Serializable]
    public class CharacterData
    {
        // Base Character Data
        public string characterName = "PlayerName"; // 캐릭터 이름
        public Tribe characterTribe = Tribe.Human; // 종족
        public int tribeIndex = 0; // 종족 ID
        public int maxHP = 100;
        public int maxMP = 100; // 최대 마력
        
        // Character's level and experience
        public int level = 1; // 레벨

        // Character Status Data
        public int strength = 5; // 힘
        public int defense = 5; // 방어
        public int speed = 5; // 속도
        public int intelligence = 5; // 지능
        public int luck = 5; // 행운
        
        // Current Character State
        public int currentHP = 100; // 현재 체력
        public int currentMP = 100; // 현재 마력
        public int currentExp; // 현재 경험치 
        
        // Character battle data
        public float attackRange = 5; // 공격 범위
        // Chracter movement data
        public float moveSpeed = 5; // 이동 속도
    }

    public class BuffData
    {
        public SkillType skillType;
        
        public int buffHp;
        public int buffMp;
        
        public int buffStr;
        public int buffDef;
        public int buffSpe;
        public int buffInt;
        public int buffLuc;
        
        public int buffDuration;
        
        public BuffData(){}
        public BuffData(SkillType type, int buffHp, int buffMp, int buffStr, int buffDef, int buffSpe, int buffInt, int buffLuc,
            int buffDuration)
        {
            this.skillType = type;
            this.buffHp = buffHp;
            this.buffMp = buffMp;
            
            this.buffStr = buffStr;
            this.buffDef = buffDef;
            this.buffSpe = buffSpe;
            this.buffInt = buffInt;
            this.buffLuc = buffLuc;
            
            this.buffDuration = buffDuration;
        }
        
        public static BuffData operator +(BuffData a, BuffData b)
        {
            var result = new BuffData();
            result.skillType = a.skillType;
            result.buffHp = a.buffHp + b.buffHp;
            result.buffMp = a.buffMp + b.buffMp;
            result.buffStr = a.buffStr + b.buffStr;
            result.buffDef = a.buffDef + b.buffDef;
            result.buffSpe = a.buffSpe + b.buffSpe;
            result.buffInt = a.buffInt + b.buffInt;
            result.buffLuc = a.buffLuc + b.buffLuc;
            result.buffDuration = a.buffDuration + b.buffDuration;
            return result;
        }
        public static BuffData operator -(BuffData a, BuffData b)
        {
            var result = new BuffData();
            result.skillType = a.skillType;
            result.buffHp = a.buffHp - b.buffHp;
            result.buffMp = a.buffMp - b.buffMp;
            result.buffStr = a.buffStr - b.buffStr;
            result.buffDef = a.buffDef - b.buffDef;
            result.buffSpe = a.buffSpe - b.buffSpe;
            result.buffInt = a.buffInt - b.buffInt;
            result.buffLuc = a.buffLuc - b.buffLuc;
            result.buffDuration = a.buffDuration - b.buffDuration;
            return result;
        }
    }
    [Serializable]
    public enum Tribe 
    {
        Human,
        Elf,
        Devil,
        Skeleton,
        Orc,
    }
}