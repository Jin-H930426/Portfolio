using System;
using System.Collections.Generic;
using JH.Portfolio.Character;
using UnityEngine;

namespace JH.Portfolio.Item
{
    public class Inventory : MonoBehaviour
    {
        public static Inventory instance;

        public List<ItemData> itemList = new List<ItemData>();
        
        public void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
                return;
            }
            
            instance = this;
        }
        
        public static void AddItem(ItemData item)
        {
            instance.itemList.Add(item);
        }
        public static void RemoveItem(ItemData item)
        {
            instance.itemList.Remove(item);
        }
        
        public static (Sprite icon, string name, string description) GetItemDisplayInfo(int index)
        {
            return (instance.itemList[index].itemSkill.skillIcon, instance.itemList[index].itemName, instance.itemList[index].itemDescription);
        }
        public static SkillData GetItemSkillData(int index)
        {
            return instance.itemList[index].itemSkill;
        }
        public static int GetItemCount()
        {
            return instance.itemList.Count;
        }
    }
}