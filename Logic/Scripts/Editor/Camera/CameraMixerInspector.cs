using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace JH.Portfolio.Camera
{
    [CustomEditor(typeof(CameraMixer))]
    [CanEditMultipleObjects]
    public class CameraMixerInspector : Editor
    {
        bool isInScene = false;
        private bool isViewSelectionCamera = false;
        private CameraBrain _cameraBrain;
        
        private SerializedProperty _weightProperty;
        private SerializedProperty _cameraStateMachines;
        
        private void OnEnable()
        {
            _weightProperty = serializedObject.FindProperty("mixWeight");
            _cameraStateMachines = serializedObject.FindProperty("cameraStateMachines");
            
            if (target is not CameraMixer mixer) return;
            
            mixer.SetListByChildren();
            
            isInScene = JHUtility.IsInScene(mixer.gameObject);
            if (!isInScene) return;
            
            _cameraBrain = FindObjectOfType<CameraBrain>();
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            DrawStateMachineWeight();

            if (!isInScene) return;
            
            _cameraBrain.SetPositionAndRotationAtEditor();
        }
        void DrawStateMachineWeight()
        {
            // Get camera state machine count
            int count = _cameraStateMachines.arraySize;
            
            // If camera state machine count is 0, show message
            if (count == 0)
            {
                EditorGUILayout.HelpBox("No camera state machine", MessageType.Info);
                return;
            }
            
            // Define vertical area 
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                EditorGUILayout.Space();
                
                isViewSelectionCamera = EditorGUILayout.Toggle("view machine weight", isViewSelectionCamera, GUILayout.Width(EditorGUIUtility.fieldWidth));
                {
                    if (isViewSelectionCamera)
                    {
                        EditorGUI.indentLevel++;
                        {
                            for (int i = 0; i < count; i++)
                            {
                                using (var element = _cameraStateMachines.GetArrayElementAtIndex(i))
                                {
                                    if (element.objectReferenceValue is CameraStateMachine machine)
                                    {
                                        DrawWeightElement(machine.name, machine.CameraPriority);
                                    }
                                }
                            }
                        }
                        EditorGUI.indentLevel--;
                    }
                }
                EditorGUILayout.Space(5);
            } EditorGUILayout.EndVertical(); 
        }
        void DrawWeightElement(string machineName, float weight)
        {
            EditorGUILayout.BeginHorizontal();
            {
                GUI.changed = false;
                EditorGUILayout.Space(25);
                var lastRect = GUILayoutUtility.GetLastRect();
                var rect = new Rect(lastRect.x, lastRect.y, lastRect.width, 24);
                
                Color current = GUI.backgroundColor;
                Color currentContent = GUI.contentColor;
                GUI.backgroundColor = Color.Lerp(Color.gray, Color.green, weight);
                GUI.contentColor = Color.white;
                EditorGUI.ProgressBar(rect, weight, machineName);
                GUI.backgroundColor = current;
                GUI.contentColor = currentContent;
                GUI.changed = true;
                lastRect = rect;
            } EditorGUILayout.EndHorizontal();
            
        }
    }
}