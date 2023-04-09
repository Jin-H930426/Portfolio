using System.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace JH
{
    using Portfolio;
    
    public class JHEditorUtility
    {
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