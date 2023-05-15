using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JH;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace JH.Portfolio.Character
{
    [CreateAssetMenu(fileName = "Character Resource", menuName = "Manager/Item/CharacterResources")]
    public class CharacterSpriteResources : ScriptableObject
    {
        public int resourceId;
        public SerializedDictionary<CharacterCustomPivot.Parts, Sprite> spriteRenderers;

        #if UNITY_EDITOR
        public Texture2D ResourceTexture2D;
        [ContextMenu("Resource Path")]
        public void GetSprite()
        {
            SetSprite(ResourceTexture2D, out spriteRenderers);
        }
        [MenuItem("Manager/Resources/Character/Create Character Sprite Resource", false)]
        public static async Task CreateSpriteAsync(MenuCommand command)
        {
            var sprites = Selection.objects.OfType<Texture2D>().ToArray();
            var manager = await CharacterCustomManager.CreateAndInstance();
            foreach (var texture2D in sprites)
            {
                await manager.AddCharacterData(texture2D);
            }
        }
        [MenuItem("Manager/Resources/Character/Create Character Sprite Resource In Directory", false)]
        public static async Task CreateSpriteAtDirectoryAsync(MenuCommand command)
        {
            var pathFileFilter = AssetDatabase.GetAssetPath(Selection.activeObject);
            var sprites = AssetDatabase.FindAssets("t:Texture2D", new[] { pathFileFilter })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<Texture2D>).ToArray();
            var manager = await CharacterCustomManager.CreateAndInstance();
            foreach (var texture2D in sprites)
            {
                await manager.AddCharacterData(texture2D);
            }
        }
        public static void SetSprite(Texture2D texture, out SerializedDictionary<CharacterCustomPivot.Parts, Sprite> spriteRenderers)
        {
            string path = UnityEditor.AssetDatabase.GetAssetPath(texture).
                Replace(".png","").Replace(".jpg","").Replace(".jpeg","").
                Replace(".tga","").Replace(".bmp","").Replace(".psd","");
            int index = path.IndexOf("Resources");
            string result = path.Substring(index + 10);
            var s = Resources.LoadAll<Sprite>(result);
            if (s.Length != 6)
            {
                Debug.LogError($"{path} - {s.Length} - Sprite Count is not 6");
                spriteRenderers = null;
                return;
            }

            spriteRenderers = new SerializedDictionary<CharacterCustomPivot.Parts, Sprite>(
                    new []{
                        CharacterCustomPivot.Parts.Head,
                        CharacterCustomPivot.Parts.Body,
                        CharacterCustomPivot.Parts.ArmL,
                        CharacterCustomPivot.Parts.ArmR,
                        CharacterCustomPivot.Parts.LegL,
                        CharacterCustomPivot.Parts.LegR,
                    });
            
            spriteRenderers[CharacterCustomPivot.Parts.Head] = s[5];
            spriteRenderers[CharacterCustomPivot.Parts.Body] = s[2];
            spriteRenderers[CharacterCustomPivot.Parts.ArmL] = s[0];
            spriteRenderers[CharacterCustomPivot.Parts.ArmR] = s[1];
            spriteRenderers[CharacterCustomPivot.Parts.LegL] = s[3];
            spriteRenderers[CharacterCustomPivot.Parts.LegR] = s[4];
        }
        #endif
    }
}