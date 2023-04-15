using System.Collections;
using UnityEngine;
using UnityEditor;

namespace JH.Portfolio.Map
{
    [CustomEditor(typeof(JH.Portfolio.Map.Map))]
    public class MapInspector : Editor
    {
        private GUIStyle boolText;
        private Vector2 currentScrollPos = Vector2.zero;

        private Map map = null;

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
            map = target as Map;
            var sc = serializedObject.FindProperty("m_Script");
            var mapName = serializedObject.FindProperty("mapName");
            var tileCosts = serializedObject.FindProperty("tileCosts");
            var filterLenght = serializedObject.FindProperty("filterLenght");
            var sizeX = serializedObject.FindProperty("sizeX");
            var sizeY = serializedObject.FindProperty("sizeY");
            var distance = serializedObject.FindProperty("distance");
            var mapData = serializedObject.FindProperty("mapData");
            var mapDataX = mapData.FindPropertyRelative("sizeX");
            var mapDataY = mapData.FindPropertyRelative("sizeY");
            var tiles = mapData.FindPropertyRelative("tiles");
            
            var visible = serializedObject.FindProperty("onVisible");

            
            serializedObject.Update();
            
            // Runtime property Draw
            DrawReadOnlyProperty(in sc, ref mapName);
            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
            EditorGUILayout.LabelField("Runtime Property", boolText);
            DrawXYField(ref sizeX, ref sizeY, ref filterLenght, ref tileCosts, map);
            DrawDistanceField(ref distance);
            DrawMap(ref tiles, ref visible, map, mapDataX.intValue, mapDataY.intValue);
            
            // Editor property Draw
            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
            EditorGUILayout.LabelField("On Editor Property", boolText);
            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
            EditorPropertDraw();
            serializedObject.ApplyModifiedProperties();
            // Check change property validation
            if (GUI.changed)
            {
                EditorUtility.SetDirty(serializedObject.targetObject);
            }
        }

        #region Runtime Property Draw 
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
        void DrawXYField(ref SerializedProperty sizeX, ref SerializedProperty sizeY, 
            ref SerializedProperty filterLenght, ref SerializedProperty tileCosts, Map map)
        {
            // header
            EditorGUILayout.LabelField("Map Setting Property", boolText);
            // set min, max value area
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Size X / Y");
                sizeX.intValue = Mathf.Clamp(sizeX.intValue, 1, 1000);
                sizeY.intValue = Mathf.Clamp(sizeY.intValue, 1, 1000);
                EditorGUILayout.PropertyField(sizeX, GUIContent.none);
                EditorGUILayout.LabelField("/", GUILayout.Width(10));
                EditorGUILayout.PropertyField(sizeY, GUIContent.none);

            } EditorGUILayout.EndHorizontal();
            // input filter lenght
            EditorGUILayout.PropertyField(filterLenght);
            // input tile cost
            EditorGUILayout.PropertyField(tileCosts);
            // set button
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Bake"))
                {
                    map.BakeMap();
                }

                if (GUILayout.Button("Clear"))
                {
                    map.ClearMap();
                }
            } EditorGUILayout.EndHorizontal();
        }
        void DrawDistanceField(ref SerializedProperty distance)
        {
            // calculate distance
            EditorGUILayout.PropertyField(distance);
        }
        void DrawMap(ref SerializedProperty tiles, ref SerializedProperty visible, Map map ,int sizeX, int sizeY)
        {
            var width = EditorGUIUtility.currentViewWidth - 30.0f;
            var height = 210.0f;
            var contentWidth = 70.0f;
            var contentHeight = 70.0f;
            var contentSize = new GUILayoutOption[2]
            {
                GUILayout.Height(contentHeight),
                GUILayout.Width(contentWidth)
            };
            
            // get tiles array size
            var arraySize = tiles.arraySize;
            visible.boolValue = EditorGUILayout.ToggleLeft("visiable map status", visible.boolValue, boolText);
            if (arraySize == 0 || sizeX * sizeY != arraySize || !visible.boolValue) return;
            
            // get scroll position and initialize scroll view width and height
            var scrollX = Mathf.FloorToInt(currentScrollPos.x / contentWidth);
            var scrollY = Mathf.FloorToInt(currentScrollPos.y / contentHeight);
            // calculate max scroll position
            var maxScrollX = Mathf.Min(scrollX + Mathf.CeilToInt(width / contentWidth) + 1, sizeX);
            var maxScrollY = Mathf.Min(scrollY + Mathf.CeilToInt(height / contentHeight) + 1, sizeY);
            // get scroll position and initialize scroll view width and height
            currentScrollPos = EditorGUILayout.BeginScrollView(currentScrollPos, GUILayout.Width(width), GUILayout.Height(height));
            {
                // create scroll view area
                // get area size for tile count and size
                // set tile size and tile count
                EditorGUILayout.BeginVertical(GUILayout.Width(sizeX * contentWidth), GUILayout.Height(sizeY * contentHeight));
                {
                    EditorGUILayout.BeginHorizontal();
                    // display tiles
                    for (int i = scrollY; i < maxScrollY; i++)
                    {
                            for (int j = scrollX; j < maxScrollX; j++)
                            {
                                var cost = map.GetTileCost(j, i);
                                
                                // 버튼형태의 tile 값을 출력한다
                                // 버튼 인덱스와 tile 값을 이용해 출력이 가능하다.
                                if (GUI.Button(new Rect((j) * contentWidth, i * contentHeight, contentWidth, contentHeight),
                                        $"{j},{i}\ncost: {cost}\n{map[j, i]}"))
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
        #endregion

        #region Editor Property Draw

        void EditorPropertDraw()
        {
            var mesh = serializedObject.FindProperty("mesh");
            var gridColor = serializedObject.FindProperty("gridColor");
            EditorGUILayout.PropertyField(mesh);
            EditorGUILayout.PropertyField(gridColor);
            
            var texture2D = serializedObject.FindProperty("_texture2D");
            var textureMaterial = serializedObject.FindProperty("_textureMaterial");

            GUI.enabled = false;
            EditorGUILayout.PropertyField(texture2D);
            EditorGUILayout.PropertyField(textureMaterial);
            GUI.enabled = true;
        }

        private void OnSceneGUI()
        {
            map?.DrawMap();
        }
        #endregion
    }
}