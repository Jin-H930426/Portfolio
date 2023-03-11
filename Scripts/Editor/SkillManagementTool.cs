using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEditor;

namespace JH.Portfolio.Character
{
    public class SkillManagementTool : EditorWindow
    {
        SkillManager _skillManager;
        private SkillData _currentSkillData;
        private Vector2 _scrollPos;
        private Vector2 _descriptionScrollPos;

        /// <summary>
        /// Open skill management tool window
        /// </summary>
        [MenuItem("Window/Skill Management Tool")]
        public static void ShowWindow()
        {
            GetWindow<SkillManagementTool>("Skill Management Tool");
        }

        private void LoadSkillManager()
        {
            if (FindAssets.FindAssetByType<SkillManager>() is SkillManager manager)
            {
                Debug.Log("Load SkillManager.asset");
                _skillManager = manager;
            }
            else
            {
                Debug.Log("Create SkillManager.asset");
                _skillManager = CreateInstance<SkillManager>();
                _skillManager.Initailization();
                AssetDatabase.CreateAsset(_skillManager, "Assets/Resources/Skill/SkillManager.asset");
                AssetDatabase.SaveAssets();
            }
        }

        private void OnGUI()
        {
            if (_skillManager == null) LoadSkillManager();
            // Draw create skill button
            if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20)))
            {
                if (CreateSkillData() is SkillData data)
                {
                    _skillManager.skillList.Add(data);
                    EditorUtility.SetDirty(_skillManager);
                }
                else
                {
                        Debug.Log($"저장 위치가 올바르지 않습니다.");
                }
            }

            // Draw skill data list
            DrawSkillDataScroll();

            // if selected skill data
            if (_currentSkillData != null)
            {
                DrawSkillData();
            }
        }

        private void DrawSkillDataScroll()
        {
            GUILayout.Box("Skill data list", GUILayout.ExpandWidth(true));
            _scrollPos =
                EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            foreach (var skillData in _skillManager.skillList)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(new GUIContent(skillData.skillName, skillData.skillIcon)))
                {
                    _currentSkillData = skillData;
                }

                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(skillData));
                    _skillManager.skillList.Remove(skillData);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    break;
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawSkillData()
        {
            if (_currentSkillData == null) return;
            EditorGUI.BeginChangeCheck();
            _currentSkillData.skillIcon =
                EditorGUILayout.ObjectField("Skill Icon", _currentSkillData.skillIcon, typeof(Texture2D), false) as
                    Texture2D;
            _currentSkillData.skillName = EditorGUILayout.TextField("Skill Name", _currentSkillData.skillName);
            _currentSkillData.skillCoolDown =
                EditorGUILayout.FloatField("Cool Time", _currentSkillData.skillCoolDown);
            _currentSkillData.skillDamage = EditorGUILayout.IntField("Damage", _currentSkillData.skillDamage);
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Description", GUILayout.Width(145));
                GUILayout.BeginVertical();
                {
                    _descriptionScrollPos = EditorGUILayout.BeginScrollView(_descriptionScrollPos);
                    _currentSkillData.description = EditorGUILayout.TextArea(_currentSkillData.description,
                        GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                    GUILayout.EndScrollView();
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        private SkillData CreateSkillData()
        {
            SkillData newData = ScriptableObject.CreateInstance<SkillData>();
            Selection.activeObject = newData;
            if (EditorUtility.SaveFilePanelInProject("Save Skill Data", newData.skillName, "asset", "Create Skill Data")
                is string path)
            {
                newData.skillName =
                    path.Substring(path.LastIndexOf('/') + 1, path.LastIndexOf('.') - path.LastIndexOf('/') - 1);
                AssetDatabase.CreateAsset(newData, path);
                Debug.Log($"skill name : {newData.skillName}, new data object name : {newData.name}");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                return newData;
            }

            return null;
        }

        private void OnDisable()
        {
            Debug.Log("Clear SkillManager.asset");
            _skillManager = null;
        }
    }
}