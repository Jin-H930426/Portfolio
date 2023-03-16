using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace JH.Portfolio.Character.Attribute
{
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyAttributeDraw : PropertyDrawer
    {
        static readonly Dictionary<ReadOnlyAttribute.ReadOnlyType, bool> readOnlyStatus = new Dictionary<ReadOnlyAttribute.ReadOnlyType, bool>
        {
            {ReadOnlyAttribute.ReadOnlyType.ReadOnly, true},
            {ReadOnlyAttribute.ReadOnlyType.ReadOnlyWhenPlaying, Application.isPlaying},
            {ReadOnlyAttribute.ReadOnlyType.ReadOnlyWhenNotPlaying, !Application.isPlaying}
        };
        
        // get the height of the property
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
        
        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Get the ReadOnlyAttribute from the property
            if (attribute is ReadOnlyAttribute readOnlyAttribute)
            {
                // If the property is read-only, draw it as disabled
                if (readOnlyStatus[readOnlyAttribute.readOnlyType])
                {
                    // Save the old GUI enabled state
                    bool wasEnabled = GUI.enabled;

                    // Set the GUI to disabled
                    GUI.enabled = false;

                    // Draw the property as disabled
                    EditorGUI.PropertyField(position, property, label, true);

                    // Restore the old GUI enabled state
                    GUI.enabled = wasEnabled;
                }
                // If the property is not read-only, draw it as normal
                else
                {
                    EditorGUI.PropertyField(position, property, label, true);
                }
            }
        }
    }
}