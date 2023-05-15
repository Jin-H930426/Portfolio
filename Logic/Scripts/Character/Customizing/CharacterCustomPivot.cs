using System;
using System.Collections;
using System.Collections.Generic;
using JH;
using UnityEngine;

namespace JH.Portfolio.Character
{
    public class CharacterCustomPivot : MonoBehaviour
    {
        [System.Serializable]
        public enum Parts
        {
            Head,
            Body,
            ArmL,
            ArmR,
            LegL,
            LegR,
            EyeBackL,
            EyeBackR,
            EyeFrontL,
            EyeFrontR,
            Hair,
            Hermit1,
            Hermit2,
            FaceHair,
            ClothBody,
            ClothArmL,
            ClothArmR,
            ClothLegL,
            ClothLegR,
            Armor,
            ShoulderL,
            ShoulderR,
            Back,
            WeaponL,
            WeaponR,
            ShieldL,
            ShieldR,
        }

        public SerializedDictionary<Parts, SpriteRenderer> customPivotDic =
            new SerializedDictionary<Parts, SpriteRenderer>();
        
        public void SetSprite(Parts parts, Sprite sprite)
        {
            customPivotDic[parts].sprite = sprite;
        }

#if UNITY_EDITOR
        public List<SpriteRenderer> spriteRendererList_Editor = new List<SpriteRenderer>();

        [ContextMenu("Find Custom Pivot")]
        private void FindSpriteRenderer_Editor()
        {
            spriteRendererList_Editor.Clear();
            GetComponentsInChildren(true, spriteRendererList_Editor);
        }

        [ContextMenu("Set Dictionary")]
        private void SetDictionary_Editor()
        {
            customPivotDic.Clear();
            foreach (Parts parts in Enum.GetValues(typeof(Parts)))
            {
                customPivotDic.Add(parts, spriteRendererList_Editor[(int)parts]);
            }
        }
#endif
    }
}