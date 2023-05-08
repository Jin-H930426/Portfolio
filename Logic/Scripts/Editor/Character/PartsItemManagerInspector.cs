using System;
using JH.Portfolio.Item;
using UnityEngine;
using UnityEditor;

namespace JH.Portfolio.Character
{
    [CustomEditor(typeof(PartsItemManager))]
    public class PartsItemManagerInspector : Editor
    {
        private PartsItemGroup viewGroup;
        private int spriteScale = 5;
        Vector2 scrollPos;
        private void OnEnable()
        {
            viewGroup = ((PartsItemManager)target).partsItemDataList[(ItemParts)0];
            scrollPos = Vector2.zero;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            PartsItemManager manager = target as PartsItemManager;
            int tapCount = manager.partsItemDataList.Count;

            //Tap button
            var width = (EditorGUIUtility.currentViewWidth - EditorGUIUtility.fieldWidth) / tapCount * 2;
            var height = EditorGUIUtility.singleLineHeight;
            EditorGUILayout.BeginVertical(GUILayout.Height(height * 2 + EditorGUIUtility.standardVerticalSpacing));
            int index = 0;
            // 4 element horizontal group
            foreach ((var key, var value) in manager.partsItemDataList)
            {
                if (index % 4 == 0)
                {
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                }

                //Tap Toggle
                if (GUILayout.Toggle(viewGroup == value, ((ItemParts)index).ToString(),
                        "Button", GUILayout.Width(width), GUILayout.Height(height)))
                {
                    viewGroup = value;
                }

                index++;
            }
            EditorGUILayout.EndHorizontal();
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(EditorGUIUtility.singleLineHeight * 16));
            if (viewGroup != null)
            {
                var groupObj = new SerializedObject(viewGroup);
                groupObj.Update();
                var h = EditorGUIUtility.singleLineHeight * spriteScale + EditorGUIUtility.standardVerticalSpacing;
                var count = viewGroup.partsItemDataList.Count;
                EditorGUILayout.BeginVertical(GUILayout.Height(h * count));
                for (var i = 0; i < count; i++)
                {
                    DrawItem(viewGroup.partsItemDataList[i]);
                }
                EditorGUILayout.EndVertical();
                if (groupObj.ApplyModifiedProperties())
                {
                    EditorUtility.SetDirty(target);
                }
                
            }
            EditorGUILayout.EndScrollView();
            
            if (serializedObject.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(target);
            }
        }

        void DrawItem(PartsItemData data)
        {
            var itemDataObj = new SerializedObject(data);
            itemDataObj.Update();
            var textrueWidth = EditorGUIUtility.singleLineHeight * (spriteScale + 1);
            var textrueHeight = EditorGUIUtility.singleLineHeight * spriteScale;
            
            var labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = Unity.Mathematics.math.min(100, EditorGUIUtility.currentViewWidth);

            EditorGUILayout.BeginHorizontal("Box", GUILayout.Height(textrueHeight), GUILayout.Width(EditorGUIUtility.currentViewWidth));
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical(GUILayout.Width(textrueWidth + EditorGUIUtility.standardVerticalSpacing * 3));
            EditorGUILayout.PropertyField(itemDataObj.FindProperty("itemIcon"), 
                GUIContent.none, GUILayout.Width(textrueWidth), GUILayout.Height(textrueHeight));
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.PropertyField(itemDataObj.FindProperty("itemID"));
            EditorGUILayout.PropertyField(itemDataObj.FindProperty("itemName"));
            EditorGUILayout.PropertyField(itemDataObj.FindProperty("itemPrice"));
            EditorGUILayout.PropertyField(itemDataObj.FindProperty("itemMaxCount"));
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            var des = itemDataObj.FindProperty("itemDescription");
            des.stringValue = EditorGUILayout.TextArea(des.stringValue, GUI.skin.textField, GUILayout.Height(textrueHeight));
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUIUtility.labelWidth = labelWidth;
            if(itemDataObj.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(data);
            }
        }
    }
}