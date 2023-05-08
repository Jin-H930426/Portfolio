using System.Collections.Generic;
using System.Linq;
using JH.Portfolio.Manager;
using UnityEngine;


namespace JH.Portfolio.Character
{
    public class CharacterInfo : MonoBehaviour
    {
        [SerializeField] private int characterID = 0;
        [SerializeField] private CharacterData characterData = new CharacterData();
        private BuffData characterbuff = new BuffData();
        List<BuffData> _buffList = new List<BuffData>();
        [SerializeField] SerializedDictionary<int, int> _coolDownDic = new SerializedDictionary<int, int>();
        [SerializeField] private SkillData[] skillList;
        
        public int Hp => characterData.currentHP;
        public int MaxHp => characterData.maxHP;
        public int Mp => characterData.currentMP;
        public int MaxMp => characterData.maxMP;
        public int Speed => characterData.speed;
        public float MoveSpeed => characterData.moveSpeed;

        public int SkillCount => skillList.Length;
        public int ItemCount => 0;

        public void SetBufferList()
        {
            for (var i = 0; i < _buffList.Count; i++)
            {
                var buffer = _buffList[i];
                buffer.buffDuration--;
                if (buffer.buffDuration <= 0)
                {
                    _buffList.Remove(buffer);
                    i--;
                    characterbuff -= buffer;
                }
            }
        }
        public void ClearBuff()
        {
            _buffList.Clear();
            characterbuff = new BuffData();
        }
        public void ClearCoolDown()
        {
            _coolDownDic.Clear();
        }


        public bool CoolDownCheck(SkillData data)
        {
            return _coolDownDic.ContainsKey(data.skillID);
        }
        public bool CostCheck(SkillData data)
        {
            return data.costHp <= Hp && data.costMp <= Mp;
        }
        public void SetCoolDown(SkillData skill)
        {
            _coolDownDic.Add(skill.skillID, skill.skillCoolDown);
        }
        public void SetCoolDownDic()
        {
            foreach (var skillData in skillList)
            {
                if (_coolDownDic.ContainsKey(skillData.skillID))
                {
                    var coolDown = _coolDownDic[skillData.skillID] - 1;

                    if (coolDown <= 0)
                    {
                        _coolDownDic.Remove(skillData.skillID);
                        continue;
                    }
                    
                    _coolDownDic[skillData.skillID] = coolDown;
                }
            }
        }

        public (int physic, int magic, string skillAnimation, int costHp, int costMp) CalculationAttackDamage(SkillData skill)
        {
            return (skill.physicDamage, skill.masicDamage, skill.animationName, skill.costHp, skill.costMp);
        }
        public (int healHp, int healMp, string skillAnimation, int costHp, int costMp) CalculationHealAmount(SkillData skill)
        {
            return (skill.physicDamage, skill.masicDamage, skill.animationName, skill.costHp, skill.costMp);
        }
        public (BuffData buff, string skillAnimation, int costHp, int costMp) CalculationBuffAmount(SkillData skill)
        {
            BuffData buff = new BuffData(skill.skillType, skill.physicDamage, skill.masicDamage,
                skill.buffStr, skill.buffDef, skill.buffSpe, skill.buffInt, skill.buffLuc, skill.skillDuration);
            return (buff, skill.animationName, skill.costHp, skill.costMp);
        } 
        
        /// <summary>
        /// 캐릭터 피격시 데미지 계산
        /// </summary>
        /// <param name="physic">물리 데미지</param>
        /// <param name="magic">마법 데미지</param>
        /// <param name="defense">캐릭터의 방어 여부</param>
        public void ApplyDefenseDamage(int physic, int magic, bool defense)
        {
            var def = defense ? characterData.defense : 0;
            
            var damage = physic + magic - def;
            characterData.currentHP -= damage;
        }
        
        /// <summary>
        /// 캐릭터가 체력 또는 마력 회복 마법을 받는 경우
        /// </summary>
        /// <param name="healHp">회복될 체력</param>
        /// <param name="healMp">회복될 마력</param>
        public void ApplyHeal(int healHp, int healMp)
        {
            characterData.currentHP += healHp;
            characterData.currentMP += healMp;
        }

        
        
        /// <summary>
        /// 캐릭터가 버프를 받을 경우
        /// </summary>
        /// <param name="buff">캐릭터가 받을 버퍼 데이터</param>
        public void ApplyBuff(BuffData buff)
        {
            _buffList.Add(buff);
            if (buff.skillType == SkillType.Buff) characterbuff += buff;
            else characterbuff -= buff;
        }

        #region AI Priority 
        public int CalculationDamagePriority(int physic, int magic, bool defense)
        {
            var def = defense ? characterData.defense : 0;

            return physic + magic - def;
        }
        public int CalculationHealPriority(int healHp, int healMp)
        {
            return healHp + healMp;
        }
        public int CalculationBuffPriority(BuffData buff)
        {
            return Mathf.Abs(buff.buffDef + buff.buffInt + buff.buffLuc + buff.buffSpe + buff.buffHp + buff.buffMp +
                    buff.buffStr) * buff.buffDuration;
        }
        #endregion

        public void CalculationCost(int costHp, int costMp)
        {
            characterData.currentHP -= costHp;
            characterData.currentMP -= costMp;
        }

        public int GetExp()
        {
            return characterData.currentExp;
        }
        public int AddExp(int exp)
        {
            characterData.currentExp += exp;
            if(characterData.currentExp >= 100)
                return characterData.currentExp - 100;
            return characterData.currentExp;
        }
        public bool CheckDeath()
        {
            return characterData.currentHP <= 0;
        }
        public string GetString()
        {
            return $"{characterData.characterName}\n" +
                   $"HP: {characterData.currentHP.ToString("000")}/{characterData.maxHP.ToString("000")}" +
                   $"\t\tMP: {characterData.currentMP.ToString("000")}/{characterData.maxMP.ToString("000")}\n" +
                   $"str: {characterData.strength.ToString("00")}\t\t\t\t\tdef:{characterData.defense.ToString("")}" +
                   $"\t\t\t\t\tspe: {characterData.speed.ToString("00")}\n" +
                   $"int:{characterData.intelligence.ToString("00")}\t\t\t\t\t\tluc: {characterData.luck.ToString("00")}\n";
        }

        public void SetResource(CharacterCustomPivot pivot)
        {
            var characterCustomManager = GameManager.ResourceManager.LoadOnResource<CharacterCustomManager>("item/CharacterCustomManager");
            var characterCustom = 
                characterCustomManager.GetCharacterCustom($"{characterData.characterTribe}_{characterData.tribeIndex}");
            
            foreach (var (key, sprite) in characterCustom.spriteRenderers)
            {
                pivot.SetSprite(key, sprite);
            }
        }
        public SkillData GetSkillData(int index)
        {
            return skillList[index];
        }
        public SkillData[] GetSkillArray()
        {
            return skillList;
        }
    }
}