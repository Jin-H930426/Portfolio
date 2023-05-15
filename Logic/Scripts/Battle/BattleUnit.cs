using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JH.Portfolio.Animation;
using JH.Portfolio.Character;
using JH.Portfolio.Manager;
using JH.Portfolio.Map;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using CharacterInfo = JH.Portfolio.Character.CharacterInfo;

namespace JH.Portfolio.Battle
{
    public class BattleUnit : MonoBehaviour, IHeapItem<BattleUnit>
    {
        const int DEFENSE_PRIORITY = 5000;
        const int ATTACK_PRIORITY = 100;
        const int HEAL_PRIORITY = 500;
        const int BUFF_PRIORITY = 300;
        const int ITEM_PRIORITY = 1000;

        private SupportAnimation _animation;
        CharacterInfo characterInfo;
        CharacterCustomPivot pivot;
        private Action _commandAction;

        public int Hp => characterInfo.Hp;
        public int Mp => characterInfo.Hp;
        public float HpPrograss => characterInfo.Hp / characterInfo.MaxHp;
        public float MpPrograss => characterInfo.Mp / characterInfo.MaxMp;

        public int SkillCount => characterInfo.SkillCount;
        public int ItemCount => characterInfo.ItemCount;

        private int _priority = 0;
        private bool _isDefense = false;

        public int GetExp() => characterInfo.GetExp();
        public int AddExp(int exp) => characterInfo.AddExp(exp);
        public bool isDead => characterInfo.CheckDeath() || !gameObject.activeSelf;

        void Init()
        {
            characterInfo = GetComponent<CharacterInfo>();
            _animation = GetComponentInChildren<SupportAnimation>();
            pivot = GetComponentInChildren<CharacterCustomPivot>();
            _animation.SetAnimation(true, 0, .1f, 0);
        }

        public (Sprite, string, string) GetSkillDisplayInfo(int index)
        {
            SkillData skill = characterInfo.GetSkillData(index);
            return (skill.skillIcon, skill.skillName, skill.description);
        }

        public SkillData GetSkillData(int index)
        {
            return characterInfo.GetSkillData(index);
        }

        public SkillData[] GetSkillArray()
        {
            return characterInfo.GetSkillArray();
        }

        public IEnumerator Command()
        {
            if (isDead)
            {
                _commandAction = null;
                yield break;
            }

            _commandAction?.Invoke();
            _commandAction = null;
            yield return new WaitForSeconds(1);
        }

        public void UnitCommandEnter(SkillData skill, bool isItem, params BattleUnit[] target)
        {
            // 캐릭터가 죽은 경우 커맨더를 입력 받지 않음
            if (isDead) return;
            // 커맨드 사용이 아이템일 경우
            _priority = isItem ? ITEM_PRIORITY : 0;

            // 커맨더를 초기화 시킴
            _commandAction = null;
            _commandAction += () => Debug.LogError($"Skill Command - {name} {skill.skillName} Action");
            // 스킬 데이터 타입을 받아와 스킬 타입에 따라 커맨더를 입력 받음
            switch (skill.skillType)
            {
                case SkillType.Magic:
                case SkillType.Physic:
                    AttackCommand(skill, target);
                    break;
                case SkillType.Heal:
                    HealCommand(skill, target);
                    break;
                case SkillType.Buff:
                case SkillType.Debuff:
                    BuffCommand(skill, target);
                    break;
            }
        }
        void AttackCommand(SkillData skill, params BattleUnit[] targets)
        {
            // 공격 커맨드 우선 순위 + character의 스피드 등
            _priority += ATTACK_PRIORITY + characterInfo.Speed;
            // 캐릭터 정보를 통해서 공격 데미지를 계산함
            var damage = characterInfo.CalculationAttackDamage(skill);
            // 커맨드를 입력 시켜줌
            _commandAction += () =>
            {
                // 모든 타겟이 죽었는지 확인합니다.
                bool allDead = true;

                // 타겟을 돌며 데미지 및 애니메이션을 입력합니다.
                foreach (var targetUnit in targets)
                {
                    if (targetUnit.isDead) continue;
                    // 모든 타겟이 죽지 않았다면 allDead를 false로 변경합니다.
                    allDead = false;
                    // 타겟 유닛에 데미제 정보를 전달해 데미지를 계산합니다.
                    targetUnit.characterInfo.ApplyDefenseDamage(damage.physic,
                        damage.magic, targetUnit._isDefense);
                    // 타겟 유닛의 애니메이션을 재생합니다.
                    targetUnit.ActionHitAni(skill);
                }

                // 모든 타겟이 죽었다면 캐릭터의 애니메이션을 실행하지 않습니다.
                if (allDead) return;

                // 쿨다운을 설정합니다.
                characterInfo.SetCoolDown(skill);
                // 코스트를 계산합니다.
                characterInfo.CalculationCost(damage.costHp, damage.costMp);
                // 스킬 애니메이션을 작동합니다.
                ActionSkillAni(skill);
            };
        }
        void HealCommand(SkillData skill, params BattleUnit[] targets)
        {
            // 회복 커맨드 우선 순위 + character의 스피드를 더 해줌
            _priority += HEAL_PRIORITY + characterInfo.Speed;
            // 스킬의 회복량을 계산해 줌
            var heal = characterInfo.CalculationHealAmount(skill);
            // 커맨드 입력
            _commandAction += () =>
            {
                foreach (var battleUnit in targets)
                {
                    if (battleUnit.isDead) continue;
                    // 타겟의 회복량을 계산함
                    battleUnit.characterInfo.ApplyHeal(heal.healHp, heal.healMp);
                    // 타겟의 애니메이션을 재생함
                    ActionHitAni(skill);
                }

                characterInfo.SetCoolDown(skill);
                characterInfo.CalculationCost(heal.costHp, heal.costMp);
                ActionSkillAni(skill);
            };
        }
        void BuffCommand(SkillData skill, params BattleUnit[] tagets)
        {
            _priority += BUFF_PRIORITY + characterInfo.Speed;
            var buff = characterInfo.CalculationBuffAmount(skill);

            _commandAction += () =>
            {
                foreach (var battleUnit in tagets)
                {
                    if (battleUnit.isDead) continue;
                    battleUnit.characterInfo.ApplyBuff(buff.buff);
                }

                characterInfo.SetCoolDown(skill);
                characterInfo.CalculationCost(buff.costHp, buff.costMp);
                ActionSkillAni(skill);
            };
        }
        public void DefenseCommand()
        {
            _priority += DEFENSE_PRIORITY + characterInfo.Speed;
            _commandAction = () =>
            {
                _isDefense = true;
                ActionDefenseAni();
            };
        }

        public bool CoolDownCheck(SkillData data) => characterInfo.CoolDownCheck(data);
        public bool CostCheck(SkillData data) => characterInfo.CostCheck(data);

        public void ActionAnimationIdle()
        {
            if (!isDead) _animation.PlayAnimation("idle");
        }

        public void ActionDefenseAni()
        {
        }

        public void ActionSkillAni(SkillData skill)
        {
            _animation.PlayAnimation(skill.animationName);
        }

        public void ActionHitAni(SkillData skill)
        {
            _animation.PlayAnimation(isDead ? "death" : "hit");
        }


        public void EndTurn()
        {
            if (isDead) return;
            // 방어 상태를 해제합니다.
            _isDefense = false;
            ActionAnimationIdle();
            characterInfo.SetBufferList();
            characterInfo.SetCoolDownDic();
        }

        public string GetStatus()
        {
            return characterInfo.GetString();
        }

        public void SetActive(bool value)
        {
            if (!characterInfo || !pivot) Init();
            gameObject.SetActive(value && !isDead);

            if (!value || isDead) return;
            characterInfo.SetResource(pivot);
        }

        public int CompareTo(BattleUnit other)
        {
            return _priority.CompareTo(other._priority);
        }

        public int GetSkillPriority(SkillData data, BattleUnit target)
        {
            int priority = data.skillPriority;

            switch (data.skillType)
            {
                case SkillType.Magic:
                case SkillType.Physic:
                {
                    var damage =characterInfo.CalculationAttackDamage(data);
                    priority += target.characterInfo.CalculationDamagePriority(damage.physic, damage.magic, target._isDefense);
                } break;
                case SkillType.Heal:
                    var heal = characterInfo.CalculationHealAmount(data);
                    priority += target.characterInfo.CalculationHealPriority(heal.healHp, heal.healMp);
                    break;
                case SkillType.Buff:
                case SkillType.Debuff:
                        priority += target.characterInfo.CalculationBuffPriority(characterInfo.CalculationBuffAmount(data).buff);
                    break;
            }
            
            return priority;
        }

        public int HeapIndex { get; set; }
    }
}