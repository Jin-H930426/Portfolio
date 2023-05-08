using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using MenuCommand = System.ComponentModel.Design.MenuCommand;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace JH.Portfolio.Character
{
    public class CharacterCustomManager : ScriptableObject
    {
        const string PATH = "Assets/Portfolio/2.5D/Resources/Item";
        public List<CharacterSpriteResources> characterDataList = new List<CharacterSpriteResources>();

        public CharacterSpriteResources GetCharacterCustom(string resourceName)
        {
            foreach (var characterSpriteResources in characterDataList)
            {
                if (characterSpriteResources.name == resourceName)
                {
                    return characterSpriteResources;
                }
            }
            return null;
        }

        #region Editor 
#if UNITY_EDITOR
        
        [MenuItem("Manager/Resources/Character/Create Character Custom Manager", false)]
        public static async Task<CharacterCustomManager> CreateAndInstance()
        {
            var asset = AssetDatabase.FindAssets("t:CharacterCustomManager");

            if (asset.Length == 0)
            {
                var manager = await CreateManagerAsync();
                return manager;
            }
            else
            {
                var path = AssetDatabase.GUIDToAssetPath(asset[0]);
                var manager = AssetDatabase.LoadAssetAtPath<CharacterCustomManager>(path);
                return manager;
            }
        }
        private static async Task<CharacterCustomManager> CreateManagerAsync()
        {
            if (!Directory.Exists(PATH))
            {
                await Task.Run(() => Directory.CreateDirectory(PATH));
            }
            // Create CharacterCustomManager
            var manager = CreateInstance<CharacterCustomManager>();
            AssetDatabase.CreateAsset(manager, $"{PATH}/CharacterCustomManager.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            await Task.Delay(1000);
            return manager;
        }
        public async Task AddCharacterData(Texture2D texture2D)
        {
            foreach (var characterSpriteResources in characterDataList)
            {
                if (characterSpriteResources.ResourceTexture2D == texture2D)
                {
                    Debug.LogError("Already Exist");
                    return;
                }
            }
            
            // Check directory exist
            if (!System.IO.Directory.Exists(PATH))
            {
                // Create directory
                await Task.Run(() => { System.IO.Directory.CreateDirectory(PATH); });
            }
            // Get CharacterCustomManager Instance
            
            CharacterSpriteResources.SetSprite(texture2D, out var spriteRenderers);
            if (spriteRenderers == null) return;
            var data = CreateInstance<CharacterSpriteResources>();
            data.resourceId = data.GetInstanceID();
            data.name = texture2D.name;
            data.ResourceTexture2D = texture2D;
            data.spriteRenderers = spriteRenderers;
            
            characterDataList.Add(data);
            string path = $"{PATH}/CharacterCustomManager.asset";
            AssetDatabase.AddObjectToAsset(data, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(path);
        }

        public async Task RemoveCharacterData(CharacterSpriteResources data)
        {
            if (characterDataList.Contains(data))
            {
                characterDataList.Remove(data);
                string path = $"{PATH}/CharacterCustomManager.asset";
                AssetDatabase.RemoveObjectFromAsset(data);
                AssetDatabase.SaveAssets();
                AssetDatabase.ImportAsset(path);
                await Task.Delay(1000);
            }
        }
#endif
        #endregion
        
    }
}