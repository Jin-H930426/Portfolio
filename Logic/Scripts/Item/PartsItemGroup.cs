using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace JH.Portfolio.Item
{
    using Character;
    public class PartsItemGroup : ScriptableObject
    {
        public List<PartsItemData> partsItemDataList = new List<PartsItemData>();
        
        #if UNITY_EDITOR
        public void AddItemData(PartsItemData itemData)
        {
            partsItemDataList.Add(itemData);
        }
        #endif
    }
}