using UnityEngine;

namespace JH.Portfolio.Item
{
    using Character;
    public class ItemData : ScriptableObject
    {
        public int itemID;
        public string itemName;
        public Texture2D itemIcon;
        public string itemDescription;
        public int itemPrice;
        public int itemMaxCount;
        public SkillData itemSkill;
        public virtual void SetItemData(int id, string name, Texture2D icon, string description, int price,
            int maxCount)
        {
            
        }
    }
}