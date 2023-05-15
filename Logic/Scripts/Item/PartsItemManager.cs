using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace JH.Portfolio.Item
{
    using Character;

    public enum ItemParts
    {
        Hair,
        FaceHair,
        Cloth,
        Pant,
        Helmet,
        Armor,
        Weapons,
        Back
    }

    public class PartsItemManager : ScriptableObject
    {
        const string PATH = "Assets/Portfolio/2.5D/Resources/Item";
        public SerializedDictionary<ItemParts, PartsItemGroup> partsItemDataList;

#if UNITY_EDITOR
        [MenuItem("Manager/Get Selection Path")]
        public static void GetSelectionPath()
        {
            UnityEngine.Debug.Log($"{AssetDatabase.GetAssetPath(Selection.activeObject)}");
        }
        [MenuItem("Manager/Resources/Item/Create Parts Item Manager")]
        public static async Task<PartsItemManager> CreatAndInstance()
        {
            var asset = AssetDatabase.FindAssets("t:PartsItemManager");
            if (asset.Length == 0)
            {
                var manager = await CreateManagerAsync();
                return manager;
            }
            else
            {
                var path = AssetDatabase.GUIDToAssetPath(asset[0]);
                var manager = AssetDatabase.LoadAssetAtPath<PartsItemManager>(path);
                return manager;
            }
        }
        static async Task<PartsItemManager> CreateManagerAsync()
        {
            if (!Directory.Exists(PATH))
                await Task.Run(() => Directory.CreateDirectory(PATH));

            // Create PartsItemManager
            var manager = CreateInstance<PartsItemManager>();
            var path = $"{PATH}/PartsItemManager.asset";
            AssetDatabase.CreateAsset(manager, path);

            if (manager.partsItemDataList == null)
                manager.partsItemDataList = new SerializedDictionary<ItemParts, PartsItemGroup>();

            manager.partsItemDataList.Clear();
            foreach (var itemParts in (ItemParts[])Enum.GetValues(typeof(ItemParts)))
            {
                var itemGroup = CreateInstance<PartsItemGroup>();
                itemGroup.name = itemParts.ToString();
                manager.partsItemDataList.Add(itemParts, itemGroup);
                
                AssetDatabase.AddObjectToAsset(itemGroup, manager);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return manager;
        }

        [MenuItem("Manager/Resources/Item/Create Parts Item Data At Directory")]
        public static async Task CreateItemAtDirectoryAsync()
        {
            var manager = await CreatAndInstance();
            var pathFilter = AssetDatabase.GetAssetPath(Selection.activeObject);
            foreach (var findAsset in AssetDatabase.FindAssets("t:Texture2D", new[] { pathFilter }))
            {
                var path = AssetDatabase.GUIDToAssetPath(findAsset);
                var p = path.Split('/');
                if (p.Length < 2) continue;
                var dic = p[p.Length - 2];

                await CreateItemAsync(manager, dic, path);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(); 
        }
        [MenuItem("Manager/Resources/Item/Create Parts Item Data")]
        public static async Task CreateItemAsync()
        {
            var manager = await CreatAndInstance();
            foreach (var o in Selection.objects.OfType<Texture2D>())
            {
                // parts range is a-z, A-Z
                var path = AssetDatabase.GetAssetPath(o);
                var p = path.Split("/");
                if (p.Length < 2) continue;
                var dic = p[p.Length - 2];

                await CreateItemAsync(manager, dic, path);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        
        static async Task CreateItemAsync(PartsItemManager manager, string dic, string path)
        {
            try
            {
                if (dic == "")
                {
                    return;
                }

                // dic range is a-z, A-Z
                var dicName = dic.Split("_")[1]; 

                var o = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                var group = manager.partsItemDataList[(ItemParts)Enum.Parse(typeof(ItemParts), dicName)];

                // Create ItemData
                var itemData = CreateInstance<PartsItemData>();
            
                itemData.name = $"{group.name}/{o.name}";
                itemData.SetItemData(o.GetInstanceID(), o.name, o, o.name, 100, 1);
                group.AddItemData(itemData);
                itemData.hideFlags = HideFlags.HideInHierarchy;

                AssetDatabase.AddObjectToAsset(itemData, group);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"exception : {e.Message}");
            }
        }
#endif
    }
}