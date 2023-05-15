using UnityEngine;

namespace Portfolio.Scripts.Character.Class
{
    [System.Serializable]
    public class ClassData : ScriptableObject
    {
        public ClassType classType;
        public string className = "NewClass";
        public int classLevel;
    }
    [System.Serializable]
    public enum ClassType
    {
        Basic,
        Warrior,
        Mage,
        Archer
    }
}