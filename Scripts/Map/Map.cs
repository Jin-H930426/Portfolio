using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using UnityEngine;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

namespace JH.Portfolio.Map
{
    using Astar;
    
    public class Map : MonoBehaviour
    {
        #region define
        private static Dictionary<string, Map> _maps;

        const int CAN_VISIT_TILE = (int)(Tile.Ground | Tile.Water);

        readonly int2[] DIRECTION = new int2[]
        {
            new int2(0, 1),
            new int2(1, 1),
            new int2(1, 0),
            new int2(1, -1),
            new int2(0, -1),
            new int2(-1, -1),
            new int2(-1, 0),
            new int2(-1, 1),
        };
        #endregion
        #region variable
        [SerializeField] private string mapName;
        [SerializeField] private int sizeX;
        [SerializeField] private int sizeY;
        [SerializeField] private int filterLenght = 3;
        [SerializeField] private SerializedDictionary<Tile, int> tileCosts
            = new SerializedDictionary<Tile, int>(Enum.GetValues(typeof(Tile)).Cast<Tile>().ToArray());
        [SerializeField] private Vector3 distance = Vector3.one;
        [SerializeField] private MapData mapData;
        #endregion
        #region property
        public int SizeX => sizeX;
        public int SizeY => sizeY;
        public float GroundHeight => transform.position.y;
        public Vector3 Distance => distance;
        // operator for tiles
        public Tile this[int x, int y]
        {
            get => mapData != null ? mapData.tiles[y * mapData.sizeX + x] : Tile.None;
            set
            {
                if (mapData != null) mapData.tiles[y * mapData.sizeX + x] = value;
            }
        }
        public int GetTileCost(int x, int y)
        {
            return mapData != null ? mapData.tileCosts[y * mapData.sizeX + x] : 0;
        }
        #endregion

        void Awake()
        {
            if (_maps == null)
                _maps = new Dictionary<string, Map>();
            _maps[mapName] = this;
        }
        public float3 GetWorldPosition(int x, int y)
        {
            UnityEngine.Profiling.Profiler.BeginSample("GetWorldPosition");
            float3 position = transform.position;
            var halfX = mapData.sizeX / 2;
            var halfY = mapData.sizeX / 2;
            UnityEngine.Profiling.Profiler.EndSample();
            return position + new float3((x - halfX) * distance.x, GroundHeight, (y - halfY) * distance.z);
        }
        public float3 GetWorldPosition(int2 mapPosition)
        {
            return GetWorldPosition(mapPosition.x, mapPosition.y);
        }
        public int2 GetMapPosition(float3 worldPosition)
        {
            var halfX = mapData.sizeX / 2;
            var halfY = mapData.sizeX / 2;
            var position = transform.position;

            return new(
                Mathf.RoundToInt((worldPosition.x - position.x) / distance.x + halfX),
                Mathf.RoundToInt((worldPosition.z - position.z) / distance.z + halfY)
            );
        }

        public bool OnMap(float3 worldPosition)
        {
            if (mapData == null) return false;

            return OnMap(GetMapPosition(worldPosition));
        }
        public bool OnMap(int2 mapPosition)
        {
            return OnMap(mapPosition.x, mapPosition.y);
        }
        public bool OnMap(int x, int y)
        {
            if (mapData == null) return false;

            return x >= 0 && x < mapData.sizeX && y >= 0 && y < mapData.sizeY;
        }

        public bool CanVisit(int2 mapPosition)
        {
            return CanVisit(mapPosition.x, mapPosition.y);
        }
        public bool CanVisit(int x, int y)
        {
            if (!OnMap(x, y)) return false;
            return ((int)this[x, y] & CAN_VISIT_TILE) != 0;
        }
        
        public static Map GetOnMap(float3 worldPosition)
        {
            foreach (var map in _maps.Values)
            {
                if (map.OnMap(worldPosition))
                    return map;
            }

            return null;
        }

        #region Path finding 
        public void PathFindingWithAstar(int2 startPos, int2 endPos, out List<int2> movePoints)
        {
            if (!CanVisit(startPos) || !CanVisit(endPos) || startPos.Equals(endPos))
            {
                movePoints = null;
                return;
            }

            AstarNode startNode = AstarNode.Create(startPos, tileCosts[this[startPos.x, startPos.y]]);
            AstarNode endNode = AstarNode.Create(endPos, tileCosts[this[endPos.x, endPos.y]]);
            Heap<AstarNode> openList = new Heap<AstarNode>(100);
            Dictionary<int2, AstarNode> openSet = new Dictionary<int2, AstarNode>();
            HashSet<int2> closeSet = new HashSet<int2>();
            
            openList.Push(startNode);
            openSet.Add(startNode.Pos, startNode);
            
            while (openList.Count > 0 &&   // if open list is empty, stop path finding
                   openList.Count < 30000) // if path finding is too long, stop path finding
            {
                var currentNode = openList.Pop();
                openSet.Remove(currentNode.Pos);
            
                closeSet.Add(currentNode.Pos);
            
                if (currentNode == endNode)
                {
                    movePoints = new List<int2>();
                    while (currentNode != startNode)
                    {
                        movePoints.Add(currentNode.Pos);
                        currentNode = currentNode.parent;
                    }

                    movePoints.Reverse();
                    return;
                }
            
                foreach (var dir in DIRECTION)
                {
                    // get next position
                    var nextPos = currentNode.Pos + dir;
                    // if next position can't visit or Already visited, Check next direction 
                    if (CanVisit(nextPos) == false || closeSet.Contains(nextPos))
                        continue;
            
                    // Crate next node
                    var nextNode = AstarNode.Create(nextPos, tileCosts[this[nextPos.x, nextPos.y]]);
                    // Calculate G cost
                    var newGCost = currentNode.gCost + AstarNode.GetDistance(currentNode, nextNode) 
                        + nextNode.tileCost;
                    
                    // if new G cost is lower than next node's G cost or next node is not in open list
                    if (newGCost < nextNode.gCost || !openSet.ContainsKey(nextNode.Pos))
                    {
                        nextNode.gCost = newGCost;
                        nextNode.hCost = AstarNode.GetDistance(nextNode, endNode);
                        nextNode.parent = currentNode;
            
                        if (!openSet.ContainsKey(nextNode.Pos))
                        {
                            openList.Push(nextNode);
                            openSet.Add(nextNode.Pos, nextNode);
                        }
                        else
                        {
                            openList.UpdateItem(openSet[nextNode.Pos]);
                        }
                    }
                }
            }

            movePoints = null;
        }
        public void PathFindingWithAstar(float3 startPos, float3 endPos, out List<float3> movePoints)
        {
            movePoints = null;
            PathFindingWithAstar(GetMapPosition(startPos), GetMapPosition(endPos), out var path);
            if (path == null) return;
            movePoints = path.Select((mapPos) => GetWorldPosition(mapPos)).ToList();
            endPos.y = GroundHeight;
            movePoints[movePoints.Count - 1] = endPos;
        }
        #endregion

        #region EDITOR
        // 초기 메쉬를 큐브로 설정
        [SerializeField] private Mesh mesh;
        [SerializeField] private bool onVisible = false;
        [SerializeField] private SerializedDictionary<Tile, Color> gridColor
            = new SerializedDictionary<Tile, Color>(Enum.GetValues(typeof(Tile)).Cast<Tile>().ToArray());
        [SerializeField] private Texture2D _texture2D;
        [SerializeField] private Material _textureMaterial;
        [SerializeField] private Matrix4x4 _textureMatrix;

        [ContextMenu("Initialized Map")]
        public void Init()
        {
            ClearMap();
            if (mapData == null)
                mapData = new MapData();

            mapData.Update(sizeX, sizeY);
            SetMapData();
            DrawGrid();
            mapData.UpdateTileCost(tileCosts, filterLenght);
        }

        [ContextMenu("Clear Map")]
        public void ClearMap()
        {
            if (!_texture2D) DestroyImmediate(_texture2D);
            if (!_textureMaterial) DestroyImmediate(_textureMaterial);
            
            _texture2D = null;
            _textureMaterial = null; 
            mapData.Clear();
            mapData = null;
        }
        private void SetMapData()
        {
            for (var x = 0; x < mapData.sizeX; x++)
            {
                for (var y = 0; y < mapData.sizeY; y++)
                {
                    var worldPos = GetWorldPosition(x, y);
                    
                    if(Physics.CheckBox(worldPos, Distance, Quaternion.identity,
                           LayerMask.GetMask("Element"), QueryTriggerInteraction.Collide))
                    {
                        foreach (var col in Physics.OverlapBox(worldPos, Distance, Quaternion.identity,
                            LayerMask.GetMask("Element"), QueryTriggerInteraction.Collide))
                        {
                            if (Enum.TryParse(col.transform.tag, out Tile t))
                            {
                                if (((int)t & CAN_VISIT_TILE) == 0)
                                { 
                                    this[x, y] = t;
                                    break;
                                }
                                
                                if (t != Tile.Ground)
                                {
                                    this[x, y] = t;
                                }
                            }
                        }
                    }
                }
            }
        }
        void DrawGrid()
        {
            if (!_texture2D)
            {
                _texture2D = new Texture2D(mapData.sizeX, mapData.sizeY, TextureFormat.RGBAFloat, true);              
                for (var x = 0; x < mapData.sizeX; x++)
                {
                    for (var y = 0; y < mapData.sizeY; y++)
                    {
                            _texture2D.SetPixel(x, y, gridColor[mapData.tiles[y * mapData.sizeX + x]]);
                    }
                }
                _texture2D.Apply();
                _textureMaterial = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default"));
                _textureMaterial.mainTexture = _texture2D;
                Color c = Color.white;
                c.a = 78.0f / 255.0f;
                _textureMaterial.color = c; 
                
                _textureMatrix = Matrix4x4.TRS(transform.position + Vector3.up * .2f, Quaternion.Euler(90, 0, 0),
                    new Vector3(mapData.sizeX * distance.x, mapData.sizeY * distance.z, 1));
            }
        }
        #endregion
        
    }
}