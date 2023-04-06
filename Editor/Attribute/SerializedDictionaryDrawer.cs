using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.UIElements;
using Debug = JH.Portfolio.Debug;

namespace JH
{
    [CustomPropertyDrawer(typeof(SerializedDictionaryAttribute))]
    public class SerializedDictionaryDrawer : PropertyDrawer
    {
        private ReorderableList list;
        private float height = 0;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            property.serializedObject.Update();
            height = 0;

            CreateList(property, label);
            list.DoList(position);
            // height += list.elementHeight * list.count;
            height += list.GetHeight();
            // draw addition key
            ViewAdditionKey(property, position);
            property.serializedObject.ApplyModifiedProperties();
            if (GUI.changed)
            {
                EditorUtility.SetDirty(property.serializedObject.targetObject);
            }
        }

        void CreateList(SerializedProperty property, GUIContent label)
        {
            if (list != null) return;
            
            list = new ReorderableList(property.FindPropertyRelative("keyValuePairs").serializedObject,
                property.FindPropertyRelative("keyValuePairs"),
                true, true, true, true);
            
            list.drawHeaderCallback = (Rect rect) => { EditorGUI.LabelField(rect, label); };
            
            list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                
                var element = list.serializedProperty.GetArrayElementAtIndex(index);
                var key = element.FindPropertyRelative("key");
                var value = element.FindPropertyRelative("value");
                
                var w = rect.width / 3;
                var h = EditorGUIUtility.singleLineHeight;
                var keyRect = new Rect(rect.x + 15, rect.y, w -20, 16);
                if (isActive)
                    EditorGUI.PropertyField(keyRect, key, GUIContent.none, true);
                else
                    EditorGUI.LabelField(keyRect, key.stringValue);

                var valueRect = new Rect(rect.x + w + 4, rect.y, w * 2 - 4, h);
                if (value.hasChildren && value.propertyType != SerializedPropertyType.String)
                {
                    valueRect.x += 2;
                    valueRect.width -= 2;
                    
                    var copy = value.Copy();
                    
                    while (copy.NextVisible(true))
                    {
                        if (copy.depth == value.depth + 1)
                        {
                            var copyRect = new Rect(valueRect.x, valueRect.y, valueRect.width, EditorGUI.GetPropertyHeight(copy, GUIContent.none, true));
                            EditorGUI.PropertyField(copyRect, copy, true);
                            valueRect.y += copyRect.height;
                        }

                        if (copy.depth == value.depth)
                            break;
                    }
                    // EditorGUI.PropertyField(valueRect, value, new GUIContent(value.displayName), true);
                }
                else
                {
                    EditorGUI.PropertyField(valueRect, value, GUIContent.none, true);
                }
                
            };
            list.elementHeightCallback = (int index) =>
            {
                var element = list.serializedProperty.GetArrayElementAtIndex(index);
                var value = element.FindPropertyRelative("value").Copy();
                var h = EditorGUI.GetPropertyHeight(value, GUIContent.none, true);
                value.isExpanded = true;
                var count = value.CountInProperty();
                if (count > 1)
                {
                    return h - EditorGUIUtility.singleLineHeight;
                }
                else return h;
            };
            list.onCanAddCallback = (ReorderableList l) =>
            {
                if (string.IsNullOrEmpty(property.FindPropertyRelative("additionKey").stringValue))
                {
                    return false;
                }

                var count = l.count;

                for (int i = 0; i < count; i++)
                {
                    var element = l.serializedProperty.GetArrayElementAtIndex(i);
                    var key = element.FindPropertyRelative("key");
                    if (key.stringValue == property.FindPropertyRelative("additionKey").stringValue)
                    {
                        return false;
                    }
                }

                return true;
            };
            list.onAddCallback = (ReorderableList l) =>
            {
                var index = l.serializedProperty.arraySize;
                l.serializedProperty.arraySize++;
                l.index = index;

                var element = l.serializedProperty.GetArrayElementAtIndex(index);
                var key = element.FindPropertyRelative("key");
                var value = element.FindPropertyRelative("value");
                key.stringValue = property.FindPropertyRelative("additionKey").stringValue;
            };
        }

        void ViewAdditionKey(SerializedProperty property, Rect position)
        {
            var additionwidth = position.width / 3 + 53;
            var x = position.width - additionwidth;
            var y = position.y + height - EditorGUIUtility.singleLineHeight;
            var h = EditorGUIUtility.singleLineHeight;
            var labelRect = new Rect(x - 90, y, 100, h);
            var additionKeyRect = new Rect(x, y, position.width / 3, h);
            EditorGUI.LabelField(labelRect, "Addition Key");
            EditorGUI.PropertyField(additionKeyRect, property.FindPropertyRelative("additionKey"), GUIContent.none);
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) + height;
        }
    }
}