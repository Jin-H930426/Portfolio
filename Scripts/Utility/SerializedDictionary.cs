using System;
using UnityEngine;
using System.Collections.Generic;

namespace JH
{
    [System.Serializable]
    public class SerializedDictionary<TValue> : Dictionary<string, TValue>,ISerializationCallbackReceiver
    {
        [System.Serializable]
        public struct KeyValuePair
        {
            public string key;
            public TValue value;
        }
        [SerializeField] private List<KeyValuePair> keyValuePairs = new();
        [SerializeField] private string additionKey;

        public void OnBeforeSerialize()
        {
            keyValuePairs.Clear();
            
            foreach (var kvp in this)
            {
                keyValuePairs.Add(new KeyValuePair {key = kvp.Key, value = kvp.Value});
            }
        }
        public void OnAfterDeserialize()
        {
            this.Clear();
            if (keyValuePairs != null)
            {
                for (int i = 0; i != keyValuePairs.Count; i++)
                {
                    this.Add(keyValuePairs[i].key, keyValuePairs[i].value);
                }
            }
        }
    }
}