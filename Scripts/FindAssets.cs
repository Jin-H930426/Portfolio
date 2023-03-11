using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace JH.Portfolio.Character
{
    public class FindAssets
    {
          public static List<T> FindAssetsByType<T>() where T : Object
          {
              List<T> assets = new List<T>();
              string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);  //FindAssets uses tags check documentation for more info
              foreach (string guid in guids)
              {
                  string path = AssetDatabase.GUIDToAssetPath(guid);
                  if (AssetDatabase.LoadAssetAtPath<T>(path) is T asset)
                  {
                      assets.Add(asset);
                  }
              }

              return assets;
          }
          public static T FindAssetByType<T>() where T : Object
          {
              string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);  //FindAssets uses tags check documentation for more info
              foreach (string guid in guids)
              {
                  string path = AssetDatabase.GUIDToAssetPath(guid);
                  if (AssetDatabase.LoadAssetAtPath<T>(path) is T asset)
                  {
                      return asset;
                  }
              }

              return null;
          }    
    }
}