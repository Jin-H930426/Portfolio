using System;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using Debug = JH.Portfolio.Debug;

namespace JH
{
    [System.Serializable]
    public class SerializedDictionary<TKey, TValue> : Dictionary<TKey, TValue>,ISerializationCallbackReceiver
    {
        [System.Serializable]
        public struct KeyValuePair
        {
            public TKey key;
            public TValue value;
        }
        [SerializeField] private List<KeyValuePair> keyValuePairs = new();
        [SerializeField] private TKey additionKey;
        
        public SerializedDictionary() : base()
        {
            
        }
        public SerializedDictionary(TKey[] keys) : base() 
        {
            foreach (var key in keys)
            {
                this.Add(key, default(TValue));
            }
        }

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