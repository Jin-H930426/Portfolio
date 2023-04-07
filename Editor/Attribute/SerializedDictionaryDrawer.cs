using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.UIElements;
using Debug = JH.Portfolio.Debug;

namespace JH
{
    [CustomPropertyDrawer(typeof(SerializedDictionary<,>))]
    public class SerializedDictionaryDrawer : PropertyDrawer
    {
        private ReorderableList list;
        private float height = 0;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var addKey = property.FindPropertyRelative("additionKey");
            height = 0;

            CreateList(property, label, addKey);
            list.DoList(position);
            // height += list.elementHeight * list.count;
            height += list.GetHeight();
            // draw addition key
            ViewAdditionKey(addKey, position);
            if (GUI.changed)
            {
                EditorUtility.SetDirty(property.serializedObject.targetObject);
            }
        }

        void CreateList(SerializedProperty property, GUIContent label, SerializedProperty addKey)
        {
            if (list != null) return;
            
            list = new ReorderableList(property.FindPropertyRelative("keyValuePairs").serializedObject,
                property.FindPropertyRelative("keyValuePairs"),
                true, true, true, true);

            list.drawHeaderCallback = (Rect rect) => OnDrawHeader(rect, label);

            list.drawElementCallback = OnDrawElement;
            list.elementHeightCallback = OnElementHeight;
            list.onCanAddCallback = (ReorderableList l)=>OnCanAdd(l, addKey);
            list.onAddCallback = (ReorderableList l) => OnAdd(l, addKey);
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
            DrawElement(additionKeyRect, in property);
            property.isExpanded = true;
            var count = property.Copy().CountInProperty();
            if (count > 1)
            {
                height += EditorGUI.GetPropertyHeight(property, GUIContent.none, true) - 
                       EditorGUIUtility.singleLineHeight * 2;
            }
            // EditorGUI.PropertyField(additionKeyRect, property, GUIContent.none, true);
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) + height;
        }

        #region list call back
        void OnDrawHeader(Rect rect, GUIContent label)
        {
            EditorGUI.LabelField(rect, label);
        }
        void OnDrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            // get list element
            var element = list.serializedProperty.GetArrayElementAtIndex(index);
            // get key and value
            var key = element.FindPropertyRelative("key");
            var value = element.FindPropertyRelative("value");
            // set key rect
            var w = rect.width / 3;
            var h = EditorGUIUtility.singleLineHeight;
            var keyRect = new Rect(rect.x + 15, rect.y, w -20, h);
            bool currentEnabled = GUI.enabled;
            GUI.enabled = isActive;
            // EditorGUI.PropertyField(keyRect, key, GUIContent.none, true);
            DrawElement(keyRect, in key);
            GUI.enabled = currentEnabled;
            // set value rect
            var valueRect = new Rect(rect.x + w + 4, rect.y, w * 2 - 4, h);
            DrawElement(valueRect, in value);
        }

        void DrawElement(Rect rect, in SerializedProperty property)
        {
            // Check value type is array, list, struct, class etc...
            if (property.hasVisibleChildren)
            {
                rect.x += 2;
                rect.width -= 2;
                
                var copy = property.Copy();
                
                while (copy.NextVisible(true))
                {
                    if (copy.depth == property.depth + 1)
                    {
                        var copyRect = new Rect(rect.x, rect.y, 
                            rect.width, EditorGUI.GetPropertyHeight(copy, GUIContent.none, true));
                        var labelWidth = EditorGUIUtility.labelWidth;
                        EditorGUIUtility.labelWidth = Mathf.Min(10 * (copy.displayName.Length + 1), 100);
                        EditorGUI.PropertyField(copyRect, copy ,true);
                        EditorGUIUtility.labelWidth = labelWidth;
                        rect.y += copyRect.height;
                    }

                    if (copy.depth == property.depth)
                        break;
                }
            }
            else
            {
                EditorGUI.PropertyField(rect, property, GUIContent.none, true);
            } 
        }
        float OnElementHeight(int index)
        {
            var element = list.serializedProperty.GetArrayElementAtIndex(index);
            var value = element.FindPropertyRelative("value");
            var key = element.FindPropertyRelative("key");
            value.isExpanded = true;
            key.isExpanded = true;
            var h = Mathf.Max(
                EditorGUI.GetPropertyHeight(key, GUIContent.none, true),
                EditorGUI.GetPropertyHeight(value, GUIContent.none, true));
            var count = Mathf.Max(value.CountInProperty(), key.CountInProperty());
            if (count > 1)
            {
                return h - EditorGUIUtility.singleLineHeight;
            }
            else return h;
        }
        bool OnCanAdd(ReorderableList l, SerializedProperty addKey)
        {
            // Tkey type의 key의 값이 default나 null 경우 추가 불가
            if (addKey == null ||
                (addKey.propertyType == SerializedPropertyType.String && string.IsNullOrEmpty(addKey.stringValue)) ||
                (addKey.propertyType == SerializedPropertyType.Integer && addKey.intValue == 0) ||
                (addKey.propertyType == SerializedPropertyType.Float && addKey.floatValue == 0) ||
                (addKey.propertyType == SerializedPropertyType.ObjectReference && addKey.objectReferenceValue == null))
                return false;

            var count = l.count;

            for (int i = 0; i < count; i++)
            {
                var element = l.serializedProperty.GetArrayElementAtIndex(i);
                var key = element.FindPropertyRelative("key");
                if (SerializedProperty.DataEquals(key, addKey))
                {
                    return false;
                }
            }

            return true;
        }
        void OnAdd(ReorderableList l, SerializedProperty addKey)
        {
            var index = l.serializedProperty.arraySize;
            l.serializedProperty.arraySize++;
            l.index = index;

            var element = l.serializedProperty.GetArrayElementAtIndex(index);
            var key = element.FindPropertyRelative("key");
            JHEditorUtility.CopySerializedPropertyValue(ref key,ref addKey);
        }
        #endregion
    }
}