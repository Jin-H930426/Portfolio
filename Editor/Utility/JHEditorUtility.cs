using System.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace JH
{
    using Portfolio;
    
    public class JHEditorUtility
    {
        public static bool EqualSerializedPropertyValue(SerializedProperty sp1, SerializedProperty sp2)
        {
            var sp1Type = sp2.propertyType;
            var sp2Type = sp1.propertyType;
            if (sp1Type != sp2Type)
            {
                Debug.LogError("Different type");
                return false;
            }
            
            switch(sp1Type)
            {
                case SerializedPropertyType.Boolean:
                    return sp1.boolValue == sp2.boolValue;
                case SerializedPropertyType.Bounds:
                    return sp1.boundsValue == sp2.boundsValue;
                case SerializedPropertyType.Character:
                    return sp1.intValue == sp2.intValue;
                case SerializedPropertyType.Color:
                    return sp1.colorValue == sp2.colorValue;
                case SerializedPropertyType.Enum:
                    return sp1.enumValueIndex == sp2.enumValueIndex;
                case SerializedPropertyType.Float:
                    return sp1.floatValue - sp2.floatValue < float.Epsilon;
                case SerializedPropertyType.Generic:
                {
                    if (sp1.arraySize == 0) break;
                    var arraySize = sp2.arraySize;
                    for (int i = 0; i < arraySize; i++)
                    {
                        var srcElement = sp2.GetArrayElementAtIndex(i);
                        var oriElement = sp1.GetArrayElementAtIndex(i);
                        if (!EqualSerializedPropertyValue(srcElement, oriElement))
                            return false;
                    }
                    return true;
                } 
                case SerializedPropertyType.Hash128:
                    return sp1.hash128Value == sp2.hash128Value;
                case SerializedPropertyType.ArraySize:
                case SerializedPropertyType.LayerMask:
                case SerializedPropertyType.Integer:
                    return sp1.intValue == sp2.intValue;
                case SerializedPropertyType.Quaternion:
                    return sp1.quaternionValue == sp2.quaternionValue;
                case SerializedPropertyType.Rect:
                    return sp1.rectValue == sp2.rectValue;
                case SerializedPropertyType.String:
                    return sp1.stringValue == sp2.stringValue;
                case SerializedPropertyType.Vector2:
                    return sp1.vector2Value == sp2.vector2Value;
                case SerializedPropertyType.Vector3:
                    return sp1.vector3Value == sp2.vector3Value;
                case SerializedPropertyType.BoundsInt:
                    return sp1.boundsIntValue == sp2.boundsIntValue;
                case SerializedPropertyType.ExposedReference:
                    return sp1.exposedReferenceValue == sp2.exposedReferenceValue;
                case SerializedPropertyType.ManagedReference:
                    return sp1.managedReferenceValue == sp2.managedReferenceValue;
                case SerializedPropertyType.ObjectReference:
                    return sp1.objectReferenceValue == sp2.objectReferenceValue;
                case SerializedPropertyType.RectInt:
                    return sp1.rectIntValue.x == sp2.rectIntValue.x ||
                           sp1.rectIntValue.y == sp2.rectIntValue.y ||
                           sp1.rectIntValue.width == sp2.rectIntValue.width ||
                           sp1.rectIntValue.height == sp2.rectIntValue.height;
                case SerializedPropertyType.Vector2Int:
                    return sp1.vector2IntValue == sp2.vector2IntValue;
                case SerializedPropertyType.Vector3Int:
                    return sp1.vector3IntValue == sp2.vector3IntValue;
            }

            return true;
        } 
        public static void CopySerializedPropertyValue(ref SerializedProperty ori, ref SerializedProperty src)
        {
            var srcType = src.propertyType;
            var oriType = ori.propertyType;
            if (srcType != oriType)
            {
                Debug.LogError("Different type");
                return;
            }
            
            switch(srcType)
            {
                case SerializedPropertyType.Boolean:
                    ori.boolValue = src.boolValue;
                    break;
                case SerializedPropertyType.Bounds:
                    ori.boundsValue = src.boundsValue;
                    break;
                case SerializedPropertyType.Character:
                    ori.intValue = src.intValue;
                    break;
                case SerializedPropertyType.Color:
                    ori.colorValue = src.colorValue;
                    break;
                case SerializedPropertyType.Enum:
                    ori.enumValueIndex = src.enumValueIndex;
                    break;
                case SerializedPropertyType.Float:
                    ori.floatValue = src.floatValue;
                    break;
                case SerializedPropertyType.Generic:
                {
                    var copySrc = src.Copy();
                    var copyOri = ori.Copy();

                    while (copySrc.NextVisible(true)&& copyOri.NextVisible(true))
                    {
                        if (copySrc.depth == src.depth + 1)
                        {
                            CopySerializedPropertyValue(ref copyOri, ref copySrc);
                        }

                        if (copySrc.depth == src.depth)
                            break;
                    }
                } break;
                case SerializedPropertyType.Hash128:
                    ori.hash128Value = src.hash128Value;
                    break;
                case SerializedPropertyType.ArraySize:
                case SerializedPropertyType.LayerMask:
                case SerializedPropertyType.Integer:
                    ori.intValue = src.intValue;
                    break;
                case SerializedPropertyType.Quaternion:
                    ori.quaternionValue = src.quaternionValue;
                    break;
                case SerializedPropertyType.Rect:
                    ori.rectValue = src.rectValue;
                    break;
                case SerializedPropertyType.String:
                    ori.stringValue = src.stringValue;
                    break;
                case SerializedPropertyType.Vector2:
                    ori.vector2Value = src.vector2Value;
                    break;
                case SerializedPropertyType.Vector3:
                    ori.vector3Value = src.vector3Value;
                    break;
                case SerializedPropertyType.AnimationCurve:
                    ori.animationCurveValue = src.animationCurveValue;
                    break;
                case SerializedPropertyType.BoundsInt:
                    ori.boundsIntValue = src.boundsIntValue;
                    break;
                case SerializedPropertyType.ExposedReference:
                    ori.exposedReferenceValue = src.exposedReferenceValue;
                    break;
                case SerializedPropertyType.ManagedReference:
                    ori.managedReferenceValue = src.managedReferenceValue;
                    break;
                case SerializedPropertyType.ObjectReference:
                    ori.objectReferenceValue = src.objectReferenceValue;
                    break;
                case SerializedPropertyType.RectInt:
                    ori.rectIntValue = src.rectIntValue;
                    break;
                case SerializedPropertyType.Vector2Int:
                    ori.vector2IntValue = src.vector2IntValue;
                    break;
                case SerializedPropertyType.Vector3Int:
                    ori.vector3IntValue = src.vector3IntValue;
                    break;
            }
        }
    }
}