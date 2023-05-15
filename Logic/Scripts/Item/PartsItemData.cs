using System.Linq;
using UnityEngine;

namespace JH.Portfolio.Item
{
    using Character;
    public class PartsItemData : ItemData
    {
        public CharacterCustomPivot.Parts[] pivots;
        public Sprite[] sprites;

#if  UNITY_EDITOR
        public override void SetItemData(int id, string name, Texture2D icon, string description, int price, int maxCount)
        {
            this.itemID = id;
            this.itemName = name;
            this.itemIcon = icon;
            this.itemDescription = description;
            this.itemPrice = price;
            this.itemMaxCount = maxCount;
            
            SetSpritesAndPivot();
        }
        void SetSpritesAndPivot()
        {
            if (itemIcon == null)
            {
                UnityEngine.Debug.LogWarning("Warning : Item Icon is null");
            }
            var path = UnityEditor.AssetDatabase.GetAssetPath(itemIcon);
            var spriteArray = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
            pivots = new CharacterCustomPivot.Parts[spriteArray.Length];
            sprites = new Sprite[spriteArray.Length];
            for (var i = 0; i < spriteArray.Length; i++)
            {
                sprites[i] = spriteArray[i];
            }
        }
#endif
    }
}