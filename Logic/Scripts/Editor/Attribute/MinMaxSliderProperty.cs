using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace JH
{
    [CustomPropertyDrawer(typeof(MinMaxSliderAttribute))]
    public class MinMaxSliderAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            if (property.propertyType == SerializedPropertyType.Vector2)
            {
                var minMaxSliderAttribute = attribute as MinMaxSliderAttribute;

                var min = Mathf.Clamp(property.vector2Value.x, minMaxSliderAttribute.minLimit,
                    minMaxSliderAttribute.maxLimit);
                var max = Mathf.Clamp(property.vector2Value.y, minMaxSliderAttribute.minLimit,
                    minMaxSliderAttribute.maxLimit);

                var x = position.x;
                var y = position.y;
                var fieldWidth = 50;
                var space = 5;
                var width = position.width - fieldWidth * 2 - space * 2;
                var height = position.height;
                EditorGUI.MinMaxSlider(new Rect(x, y, width - space, height), label, ref min, ref max,
                    minMaxSliderAttribute.minLimit, minMaxSliderAttribute.maxLimit);
                x += space + width;
                min = EditorGUI.FloatField(new Rect(x, y, fieldWidth, height), min);
                x += space + fieldWidth;
                max = EditorGUI.FloatField(new Rect(x, y, fieldWidth, height), max);
                property.vector2Value = new Vector2(min, max);
            }
            else
            {
                EditorGUI.LabelField(position, label.text, "Use MinMaxSlider with Vector2.");
            }

            EditorGUI.EndProperty();
        }
    }
}