using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace JH.Portfolio.Manager
{
    public class ResourceManager
    {
        public T LoadOnResource<T>(string path) where T : Object
        {
            return Resources.Load<T>(path);
        }
        public T InstantiateOnResource<T>(string path, Transform parent = null) where T : Object
        {
            var obj = LoadOnResource<T>(path);
            if (obj == null)
            {
                Debug.LogError($"Instantiate Error: {path} is not found.");
                return null;
            }
            return Object.Instantiate(obj, parent);
        }
        public T InstantiateOnResource<T>(string path, Vector3 position, Quaternion rotation, Transform parent = null) where T : Object
        {
            var obj = LoadOnResource<T>(path);
            if (obj == null)
            {
                Debug.LogError($"Instantiate Error: {path} is not found.");
                return null;
            }
            return Object.Instantiate(obj, position, rotation, parent);
        }
        public T InstantiateOnResource<T>(string path, Vector3 position, Transform parent = null) where T : Object
        {
            var obj = LoadOnResource<T>(path);
            if (obj == null)
            {
                Debug.LogError($"Instantiate Error: {path} is not found.");
                return null;
            }
            return Object.Instantiate(obj, position, Quaternion.identity, parent);
        }
        
  
        public T LoadOnAddressable<T> (string path) where T : Object
        {
            return null;
        }
        
        public void Destroy(Object obj)
        {
            if (obj == null)
            {
                Debug.LogError($"Destroy Error: {obj} is null.");
                return;
            }
            Object.Destroy(obj);
        }
        
                

        
    }
}