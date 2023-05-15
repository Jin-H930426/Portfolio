using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JH.Portfolio.Battle;
using JH.Portfolio.Character;
using JH.Portfolio.Map;
using UnityEngine;
using Debug = JH.Portfolio.Debug;

public class BattleAI : MonoBehaviour
{
private const int RANGEALLBITMASK = (int)SkillRange.TeamAll | (int)SkillRange.EnemyAll;
private const int ALLYBITMASK = (int)SkillRange.Self | (int)SkillRange.TeamAll | (int)SkillRange.TeamUnit;
private const int TARGETBITMASK = (int)SkillRange.TeamUnit | (int)SkillRange.EnemyUnit;

private BattleManager manager;
public bool isPlayer;
private BattleUnit[] _allyUnits;
private BattleUnit[] _enemyUnits;


public void Init(BattleManager _manager)
{
    manager = _manager;
    if (isPlayer)
    {
        _allyUnits = _manager.playerUnits;
        _enemyUnits = _manager.enemyUnits;
    }
    else
    {
        _allyUnits = _manager.enemyUnits;
        _enemyUnits = _manager.playerUnits;
    }
}

public void Init(BattleManager _manager, bool _isPlayer)
{
    isPlayer = _isPlayer;
    Init(_manager);
}

public void AI()
{
    foreach (var unit in _allyUnits)
    {
        // Unit이 사망했거나 비활성화 상태라면 스킵
        if (unit.isDead) continue;

        // 우선도가 높은 스킬과 타겟을 저장할 공간
        int bestPriority = int.MinValue;
        SkillData bestSkill = null;
        BattleUnit[] bestTarget = null;

        // 스킬을 순회하면서 우선도를 비교함
        for (int i = 0; i < unit.SkillCount; i++)
        {
            var skill = unit.GetSkillData(i);
            // 스킬 쿨다운 중이면 넘어감
            if(unit.CoolDownCheck(skill)) continue;
            // 코스트가 부족 하면 넘어감
            if (!unit.CostCheck(skill)) continue;
            
            // 스킬의 우선도와 타겟을 가져옴
            var result = GetPriorityAndTargets(unit, skill);

            // 우선도가 높은 스킬을 저장함
            if (result.priority > bestPriority)
            {
                bestPriority = result.priority;
                bestSkill = skill;
                bestTarget = result.target;
            }
        }
        // 우선도가 가장 높은 스킬을 unit의 커맨더에 저장함
        manager.BattleCommandSetting(bestSkill, unit, false, bestTarget);
    }
}

(int priority, BattleUnit[] target) GetPriorityAndTargets(BattleUnit unit,
    SkillData skill)
{
    // 타겟이 아군 인지 체크
    bool ally = ((int)skill.range & ALLYBITMASK) != 0;
    // 타겟을 지정하는지 체크
    bool targetable = ((int)skill.range & TARGETBITMASK) != 0;

    if (ally) // 아군인 경우
    {
        if (skill.range == SkillRange.Self) // 자신을 타겟으로 지정하는 경우
        {
            return (unit.GetSkillPriority(skill, unit), new[] { unit });
        }

        var targetArray = _allyUnits;
        // 아군 배열에서 타겟을 찾아 priority를 계산함
        return ResultCalculate(targetArray);
    }
    else // 적군인 경우
    {
        var targetArray = _enemyUnits;
        // 적군의 배열에서 타겟을 찾아 priority를 계산함
        return ResultCalculate(targetArray);
    }
    
    
    (int, BattleUnit[]) ResultCalculate(BattleUnit[] targetArray)
    {
        var sumPriority = 0; // 계산한 priority의 총량
        var bestPriority = int.MinValue; // 가장 높은 priority
        BattleUnit bestTarget = null; // 가장 높은 priority를 가진 타겟
        int aliveTargetSum = 0; // 살아있는 타겟의 수 - priority의 평균을 구하는데 사용

        foreach (var targetUnit in targetArray)
        {
            if (targetUnit.isDead) continue;
            var priority = unit.GetSkillPriority(skill, targetUnit);
            
            sumPriority += priority;
            aliveTargetSum++;
            
            // 우선도가 높은 타겟을 저장함
            if (priority > bestPriority)
            {
                bestTarget = targetUnit;
                bestPriority = priority;
            }
            // bestTarget이 null이면 아직 타겟을 지정하지 않은 것이므로 타겟을 지정함
            if (bestTarget == null) 
            {
                bestTarget = targetUnit;
            }
        }
         
        if (targetable) // 타겟을 지정하는 스킬
        {
            // 타겟과 최우선 프로 퍼티를 반환함
            return (bestPriority, new[] { bestTarget } );
        }
        else // 전체 유닛을 경으
        {
            // 전체 리스트와 평균 우선로들 반환함
            return (sumPriority/aliveTargetSum, targetArray);
        }
    }
}


}