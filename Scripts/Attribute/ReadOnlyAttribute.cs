using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JH
{
    /// <summary>
    /// ReadOnly Attribute
    /// You can use this attribute to make a variable read-only in the inspector.
    /// You can also use it to make a variable read-only when the game is playing.
    /// You can also use it to make a variable read-only when the game is not playing.
    /// You can use it like this: [ReadOnly] int a or [ReadOnly(ReadOnlyAttribute.ReadOnlyType)] int a
    /// </summary>
    public class ReadOnlyAttribute : PropertyAttribute
    {
        public enum ReadOnlyType
        {
            ReadOnly,
            ReadOnlyWhenPlaying,
            ReadOnlyWhenNotPlaying
        } 
        public ReadOnlyType readOnlyType { get; private set; }
        
        public ReadOnlyAttribute()
        {
            readOnlyType = ReadOnlyType.ReadOnly;
        }

        public ReadOnlyAttribute(ReadOnlyType readOnlyWhenPlaying)
        {
            readOnlyType = readOnlyWhenPlaying;
        }
    }
}