using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace JH.Portfolio.Map
{
    [CustomEditor(typeof(JH.Portfolio.Map.Map))]
    public class MapInspector : Editor
    {
        private GUIStyle boolText;
        public Vector2 currentScrollPos = Vector2.zero;

        public void OnEnable()
        {
            boolText = new GUIStyle()
            {
                richText = true,
                fontStyle = FontStyle.Bold,
                normal = new GUIStyleState() { textColor = Color.white }
            };
        }

        public override void OnInspectorGUI()
        {
            var sc = serializedObject.FindProperty("m_Script");
            var map = target as Map;
            var mapName = serializedObject.FindProperty("mapName");
            var sizeX = serializedObject.FindProperty("sizeX");
            var sizeY = serializedObject.FindProperty("sizeY");
            var distance = serializedObject.FindProperty("distance");
            var mapData = serializedObject.FindProperty("mapData");
            var mapDataX = mapData.FindPropertyRelative("sizeX");
            var mapDataY = mapData.FindPropertyRelative("sizeY");
            var tiles = mapData.FindPropertyRelative("tiles");
            var onVisible = serializedObject.FindProperty("onVisible");

            
            serializedObject.Update();
            // 자기 자신을 출력한다
            DrawReadOnlyProperty(in sc, ref mapName);
            
            DrawXYField(ref sizeX, ref sizeY, map);
            DrawDistanceField(ref distance);
            DrawMap(ref tiles, ref onVisible, map, mapDataX.intValue, mapDataY.intValue);
            
            serializedObject.ApplyModifiedProperties();
            if (GUI.changed)
            {
                
                EditorUtility.SetDirty(serializedObject.targetObject);
            }
        }

        void DrawReadOnlyProperty(in SerializedProperty sc, ref SerializedProperty mapName)
        {
            bool enable = GUI.enabled;
            GUI.enabled = false;
            EditorGUILayout.PropertyField(sc);
            var sceneName = JHUtility.GetSceneName(target as Map);
            if (mapName.stringValue != sceneName)
            {
                mapName.stringValue = sceneName;
            }
            EditorGUILayout.PropertyField(mapName);
            GUI.enabled = enable; 
        }
        void DrawXYField(ref SerializedProperty sizeX, ref SerializedProperty sizeY, Map map)
        {
            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
            // header
            EditorGUILayout.LabelField("Map setting property", boolText);
            // Map의 크기를 설정하는 변수 입력 부분
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Size X / Y");
                sizeX.intValue = Mathf.Clamp(sizeX.intValue, 1, 100);
                sizeY.intValue = Mathf.Clamp(sizeY.intValue, 1, 100);
                EditorGUILayout.PropertyField(sizeX, GUIContent.none);
                EditorGUILayout.LabelField("/", GUILayout.Width(10));
                EditorGUILayout.PropertyField(sizeY, GUIContent.none);

            } EditorGUILayout.EndHorizontal();
            
            // Map의 초기화 및 클리어 함수 동작 부분
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("init"))
                {
                    map.Init();
                }

                if (GUILayout.Button("clear"))
                {
                    map.Clear();
                }
            } EditorGUILayout.EndHorizontal();
        }
        void DrawDistanceField(ref SerializedProperty distance)
        {
            // Map의 타일간 거리를 설정하는 변수 입력 부분
            EditorGUILayout.PropertyField(distance);
        }
        void DrawMap(ref SerializedProperty tiles, ref SerializedProperty onVisible ,Map map ,int sizeX, int sizeY)
        {
            var width = EditorGUIUtility.currentViewWidth - 30.0f;
            var height = 210.0f;
            var contentWidth = 50.0f;
            var contentHeight = 50.0f;
            var contentSize = new GUILayoutOption[2]
            {
                GUILayout.Height(contentHeight),
                GUILayout.Width(contentWidth)
            };
            
            // tiles의 배열 크기를 받아온다
            var arraySize = tiles.arraySize;
            onVisible.boolValue = EditorGUILayout.ToggleLeft("visiable map status", onVisible.boolValue, boolText);
            if (arraySize == 0 || sizeX * sizeY != arraySize || !onVisible.boolValue) return;
            
            // 현재 스크롤 위치를 받아와 출력할 tiles의 시작 position을 계산한다
            var scrollX = Mathf.FloorToInt(currentScrollPos.x / contentWidth);
            var scrollY = Mathf.FloorToInt(currentScrollPos.y / contentHeight);
            // 출력될 tiles의 최대 position를 계산한다.
            var maxScrollX = Mathf.Min(scrollX + Mathf.CeilToInt(width / contentWidth) + 1, sizeX);
            var maxScrollY = Mathf.Min(scrollY + Mathf.CeilToInt(height / contentHeight) + 1, sizeY);
            // get scroll position and initialize scroll view width and height
            currentScrollPos = EditorGUILayout.BeginScrollView(currentScrollPos, GUILayout.Width(width), GUILayout.Height(height));
            {
                // Tiles를 출력할 Scroll 영역을 만든다
                // 각 타일의 크기는 contentWeight, contentHeight이다.
                // 타일의 크기와 개수를 이용해 영역의 크기를 초기화 한다.
                EditorGUILayout.BeginVertical(GUILayout.Width(sizeX * contentWidth), GUILayout.Height(sizeY * contentHeight));
                {
                    EditorGUILayout.BeginHorizontal();
                    // 계산한 위치를 기준으로 tiles를 출력한다.
                    for (int i = scrollY; i < maxScrollY; i++)
                    {
                            for (int j = scrollX; j < maxScrollX; j++)
                            {
                                // 버튼형태의 tile 값을 출력한다
                                // 버튼 인덱스와 tile 값을 이용해 출력이 가능하다.
                                if (GUI.Button(new Rect((j) * contentWidth, i * contentHeight, contentWidth, contentHeight),
                                        $"{j},{i}\n{map[j, i]}"))
                                {
                                    var tile = tiles.GetArrayElementAtIndex(i * sizeX + j);
                                    // 버튼을 누르면 다음 타일로 변경한다.
                                    tile.enumValueIndex = (tile.enumValueIndex + 1) % tile.enumDisplayNames.Length;
                                }
                            }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndScrollView();
        }
    }
}