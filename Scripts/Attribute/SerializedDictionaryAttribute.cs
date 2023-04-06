using UnityEditor;
using UnityEngine;

namespace JH
{
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class SerializedDictionaryAttribute : PropertyAttribute
    { 
        public bool Visible = false;
    }
}