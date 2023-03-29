using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JH.Portfolio.Character 
{
    public abstract class CharacterStatus : ScriptableObject
    {
        public enum CharacterType
        {
            Player,
            Enemy,
            NPC
        }
        [SerializeField] private CharacterType _characterType;
        [SerializeField] private string _characterName;
        [SerializeField] private int _level;
        [SerializeField] private int _experience;
        
        [SerializeField] private int _maxHp;
        [SerializeField] private int _currentHp;
        [SerializeField] private int _maxMp;
        [SerializeField] private int _currentMp;
        
        [SerializeField] private int _attack;
        [SerializeField] private int _defense;
        [SerializeField] private int _magicAttack;
        [SerializeField] private int _magicDefense;
        
        [SerializeField] private int _speed;
        [SerializeField] private int _luck;
        [SerializeField] private int _critical;
        [SerializeField] private int _criticalDamage;
        [SerializeField] private int _accuracy;
        [SerializeField] private int _evasion;
        [SerializeField] private int _statePoint;
        
        [SerializeField] private int _skillPoint;
        [SerializeField] private int _gold;
        
        [SerializeField] private int _itemCount;
        [SerializeField] private int _itemMaxCount;

        [SerializeField] private int _questCount;
        [SerializeField] private int _questMaxCount;
        [SerializeField] private int _questCompleteCount;
        [SerializeField] private int _questCompleteMaxCount;
        [SerializeField] private int _questProgressCount;

        public CharacterType characterType { get => _characterType; }
        public string characterName { get => _characterName; }
        public int level { get => _level; }
        public int experience { get => _experience; }
        
        public int maxHp { get => _maxHp; }
        public int currentHp { get => _currentHp; }
        public int maxMp { get => _maxMp; }
        public int currentMp { get => _currentMp; }
        
        public int attack { get => _attack; }
        public int defense { get => _defense; }
        public int magicAttack { get => _magicAttack; }
        public int magicDefense { get => _magicDefense; }
        
        public int speed { get => _speed; }
        public int luck { get => _luck; }
        public int critical { get => _critical; }
        public int criticalDamage { get => _criticalDamage; }
        public int accuracy { get => _accuracy; }
        public int evasion { get => _evasion; }
        
        public int statePoint { get => _statePoint; }
        public int skillPoint { get => _skillPoint; }
        public int gold { get => _gold; }
        
        public int itemCount { get => _itemCount; }
        public int itemMaxCount { get => _itemMaxCount; }
        
        public int questCount { get => _questCount; }
        public int questMaxCount { get => _questMaxCount; }
        public int questCompleteCount { get => _questCompleteCount; }
        public int questCompleteMaxCount { get => _questCompleteMaxCount; }
        public int questProgressCount { get => _questProgressCount; }
    } 
}

