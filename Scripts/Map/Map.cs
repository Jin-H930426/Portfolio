using System;
using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

namespace JH.Portfolio.Map
{
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

        [System.Serializable]
        public enum Tile
        {
            None = 0,
            Ground = 1,
            Water = 2,
            Build = 4,
            DeepWater = 8,
        }

        [System.Serializable]
        public class MapData
        {
            public int sizeX;
            public int sizeY;
            public Tile[] tiles;

            public void Update(int sizeX, int sizeY)
            {
                var currentX = this.sizeX;
                var currentY = this.sizeY;
                var currentTiles = this.tiles;

                this.sizeX = sizeX;
                this.sizeY = sizeY;
                tiles = new Tile[sizeX * sizeY];

                for (int x = 0; x < sizeX; x++)
                {
                    for (int y = 0; y < sizeY; y++)
                    {
                        if (currentTiles != null && x < currentX && y < currentY)
                        {
                            tiles[y * sizeX + x] = currentTiles[y * currentX + x];
                            continue;
                        }

                        tiles[y * sizeX + x] = Tile.Ground;
                    }
                }
            }

            public void Clear()
            {
                sizeX = 0;
                sizeY = 0;
                tiles = null;
            }
        }

        #endregion
        #region variable

        [SerializeField] private string mapName;
        [SerializeField] private int sizeX;
        [SerializeField] private int sizeY;
        [SerializeField] private Vector3 distance = Vector3.one;
        [SerializeField] private MapData mapData;

        #endregion
        #region property
        public int SizeX => Mathf.Min(sizeX, 100);
        public int SizeY => Mathf.Min(sizeY, 100);
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
            return position + new float3((x - halfX) * distance.x, 0, (y - halfY) * distance.z);
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
        
        public static Map GetOnMap(Vector3 worldPosition)
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

            AstarNode startNode = AstarNode.Create(startPos, true);
            AstarNode endNode = AstarNode.Create(endPos, true);
            Debug.Log("startNode : " + startNode.pos + " endNode : " + endNode.pos + "");
            List<AstarNode> openList = new List<AstarNode>();
            HashSet<int2> closeList = new HashSet<int2>();
            openList.Add(startNode);
            
            while (openList.Count > 0 && openList.Count < 30000)
            {
                var currentNode = openList[0];
                for (int i = 0; i < openList.Count; i++)
                {
                    if (openList[i].fCost < currentNode.fCost || openList[i].fCost == currentNode.fCost &&
                        openList[i].hCost < currentNode.hCost)
                        currentNode = openList[i];
                }

                openList.Remove(currentNode);
                closeList.Add(currentNode.pos);

                if (currentNode == endNode)
                {
                    movePoints = new List<int2>();
                    while (currentNode != startNode)
                    {
                        movePoints.Add(currentNode.pos);
                        currentNode = currentNode.parent;
                    }
                    Debug.Log(openList.Count);
                    return;
                }

                foreach (var dir in DIRECTION)
                {
                    var nextPos = currentNode.pos + dir;

                    if (CanVisit(nextPos) == false)
                        continue;
                    if (closeList.Contains(nextPos))
                        continue;


                    var nextNode = AstarNode.Create(nextPos, true);
                    var newMovementCostToNextNode = currentNode.gCost + AstarNode.GetDistance(currentNode, nextNode);
                    if (newMovementCostToNextNode < nextNode.gCost || !openList.Contains(nextNode))
                    {
                        nextNode.gCost = newMovementCostToNextNode;
                        nextNode.hCost = AstarNode.GetDistance(nextNode, endNode);
                        nextNode.parent = currentNode;

                        if (!openList.Contains(nextNode))
                            openList.Add(nextNode);
                    }
                }
            }

            movePoints = null;
        }
        

        #endregion

        #region EDITOR
        // 초기 메쉬를 큐브로 설정
        [SerializeField] private Mesh mesh;
        [SerializeField] private bool onVisible = false;
        [SerializeField] private SerializedDictionary<Tile, Color> gridColor = new SerializedDictionary<Tile, Color>();
        [SerializeField] private Texture2D _texture2D;
        [SerializeField] private Material _textureMaterial;
        [SerializeField] private Matrix4x4 _textureMatrix;
        private readonly Tile[] _tiles = new[]
        {
            Tile.None,
            Tile.Ground,
            Tile.Water,
            Tile.Build,
            Tile.DeepWater
        };

        private float fps = 1f / 20f;
        private float time = 0f;

        [ContextMenu("Initialized Map")]
        public void Init()
        {
            ClearMap();
            if (mapData == null)
                mapData = new MapData();

            mapData.Update(sizeX, sizeY);
            SetMapData();
            DrawGrid();
        }

        [ContextMenu("Clear Map")]
        public void ClearMap()
        {
            if (_texture2D != null) DestroyImmediate(_texture2D);
            if (_textureMaterial != null) DestroyImmediate(_textureMaterial);
            
            _texture2D = null;
            _textureMaterial = null; 
            mapData.Clear();
            mapData = null;
        }
        private void SetMapData()
        {
            var colliders = GetComponentsInChildren<Collider>();
            
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
                                else if (t != Tile.Ground)
                                {
                                    this[x, y] = t;
                                    continue;
                                }
                            }
                        }
                    }
                }
            }
        }


        private void OnCollisionEnter(Collision collision)
        {
            Debug.Log("enter");
        }
        void DrawGrid()
        {
            if (_texture2D == null)
            {
                _texture2D = new Texture2D(mapData.sizeX, mapData.sizeY, TextureFormat.RGBAFloat, true);              
                for (var x = 0; x < mapData.sizeX; x++)
                {
                    for (var y = 0; y < mapData.sizeY; y++)
                    {
                        try
                        {
                            _texture2D.SetPixel(x, y, gridColor[mapData.tiles[y * mapData.sizeX + x]]);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError(e);
                            Debug.Log($"pos : {x}, {y}");
                        }

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

        #region Job
        [BurstCompile]
        struct GetPositionJob : IJobParallelFor
        {
            public int SizeX;
            public int SizeY;
            public float3 Distance;
            public float3 Position;
            public NativeArray<float3> Result;

            public void Execute(int index)
            {
                var x = index % SizeX;
                var y = index / SizeX;
                var halfX = SizeX / 2;
                var halfY = SizeY / 2;
                var pos = Position + new float3((x - halfX) * Distance.x, 0, (y - halfY) * Distance.z);
                Result[index] = pos;
            }
        }
        [BurstCompile]
        struct CalculateMatrixJob : IJobParallelFor
        {
            public Vector3 Distance;
            public NativeArray<float3> Positions;
            public NativeArray<Matrix4x4> Result;

            public void Execute(int index)
            {
                Result[index] = Matrix4x4.TRS(Positions[index], Quaternion.identity, Distance);
            }
        }
        #endregion
    }
}