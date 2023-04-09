using System;
using System.Collections;
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
        const int CAN_VISIT_TILE = (int)(Tile.Ground | Tile.Water);
        readonly int2[] DIRECTION = new int2[]
        {
            new int2(0, 1),
            new int2(1,1),
            new int2(1, 0),
            new int2(1,-1),
            new int2(0, -1),
            new int2(-1,-1),
            new int2(-1, 0),
            new int2(-1,1),
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

        private static Dictionary<string, Map> _maps;
        #region variable
        [SerializeField] private string mapName;
        [SerializeField] private int sizeX;
        [SerializeField] private int sizeY;
        [SerializeField] private Vector3 distance = Vector3.one;
        [SerializeField] private MapData mapData;
        #endregion
        
        
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
        
        #region EDITOR
        // 초기 메쉬를 큐브로 설정
        [SerializeField] private Mesh mesh;
        [SerializeField] private bool onVisible = false;
        [SerializeField] private SerializedDictionary<Tile, Material> gridColor = new SerializedDictionary<Tile, Material>();

        private Tile[] _tiles = new[]
        {
            Tile.None,
            Tile.Ground,
            Tile.Water,
            Tile.Build,
            Tile.DeepWater
        };
        private Dictionary<Tile, List<Matrix4x4>> matrix4X4s = null;
        
        [ContextMenu("Initialized Map")]
        public void Init()
        {
            if(mapData == null)
                mapData = new MapData();
            
            mapData.Update(sizeX, sizeY);
            // matrix4X4s를 초기화한다.
            matrix4X4s = null;
            SetMapData();
        }
        [ContextMenu("Clear Map")]
        public void Clear()
        {
            // matrix4X4s를 초기화한다.
            matrix4X4s = null;
            // tiles를 초기화한다.
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
                    foreach (var c in colliders)
                    {
                        if (c.gameObject.layer != LayerMask.NameToLayer("Element")) continue;
                        if (c.bounds.Contains(worldPos))
                        {
                            if (Enum.TryParse(c.tag, out Tile tile) && (tile == Tile.Build || tile == Tile.DeepWater))
                            {
                                mapData.tiles[y * mapData.sizeX + x] = tile;
                                break;
                            }
                        }
                    }
                }
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            if (mapData == null || (onVisible && !Application.isPlaying)) return;
            
            DrawGrid();
        }
        private void OnDrawGizmos()
        {
            if (mapData == null || !onVisible || Application.isPlaying) return;
            
            DrawGrid();
        }
        void DrawGrid()
        {
            // draw grid
            Color currentColor = Gizmos.color; 
            var length = mapData.sizeX * mapData.sizeY;
            var enumCount = _tiles.Length;
            if (matrix4X4s == null)
            {
                // init native arrays
                var positionArray = new NativeArray<float3>(length, Allocator.TempJob);
                var matrixArray = new NativeArray<Matrix4x4>(length, Allocator.TempJob);
                // Calculate position array
                var positionJob = new GetPositionJob()
                {
                    SizeX = mapData.sizeX,
                    SizeY = mapData.sizeY,
                    Distance = distance,
                    Position = transform.position,
                    Result = positionArray,
                };
                var positionHandle = positionJob.Schedule(length, 100);
                // wait calculate position array
                positionHandle.Complete();
                
                // Calculate matrix array
                var matrixJob = new CalculateMatrixJob()
                {
                    distance = distance,
                    Positions = positionArray,
                    Result = matrixArray,
                };
                var matrixHandle = matrixJob.Schedule(length, 100);
                // wait calculate matrix array
                matrixHandle.Complete();

                matrix4X4s = new Dictionary<Tile, List<Matrix4x4>>();

                for (int i = 0; i < enumCount; i++)
                {
                    matrix4X4s[ _tiles[i]] = new List<Matrix4x4>();
                }
                for (int i = 0; i < length; i++)
                {
                    var x = i % mapData.sizeX;
                    var y = i / mapData.sizeX;
                    
                    matrix4X4s[this[x, y]].Add(matrixArray[i]);
                }
                
                // clear native arrays
                matrixArray.Dispose();
                positionArray.Dispose();
            }
            
            // scene view camera position
            var cameraPosition = UnityEditor.SceneView.lastActiveSceneView.camera.transform.position;
            for (int e = 0; e < enumCount; e++)
            {
                var tile = _tiles[e];
                
                var m = matrix4X4s[tile].ToArray();
                var matrixLenght = m.Length;
                int instanceCount = matrixLenght / 1023 + 1;
                for (int i = 0; i < instanceCount; i++)
                {
                    int count = Mathf.Min(1023, matrixLenght - i * 1023);
                    var start = i * 1023;
                    var end = start + count;
                    Graphics.DrawMeshInstanced(mesh, 0, gridColor[tile], m[start .. end]);
                } 
            }
            
            
            Gizmos.color = currentColor;
        }

        public void PathFindingWithAstar(int2 startPos, int2 EndPos, out List<int2> movePoints)
        {
            if (!CanVisit(startPos) || !CanVisit(EndPos) || startPos.Equals(EndPos))
            {
                movePoints = null;
                return;
            }
            
            AstarNode startNode = AstarNode.Create(startPos, true);
            AstarNode endNode = AstarNode.Create(EndPos, true);
            
            List<AstarNode> openList = new List<AstarNode>();
            HashSet<int2> closeList = new HashSet<int2>();
            openList.Add(startNode);
            
            while (openList.Count > 0)
            {
                var currentNode = openList[0];
                for (int i = 0; i < openList.Count; i++)
                {
                    if(openList[i].fCost < currentNode.fCost || openList[i].fCost == currentNode.fCost && openList[i].hCost < currentNode.hCost)
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
                    return;
                }

                foreach (var dir in DIRECTION)
                {
                    var nextPos = currentNode.pos + dir;
                    
                    if (OnMap(nextPos) == false)
                        continue;
                    if (((int)this[nextPos.x,nextPos.y] & CAN_VISIT_TILE) == 0)
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
                        
                        if(!openList.Contains(nextNode))
                            openList.Add(nextNode);
                    }
                }
            }
            
            movePoints = null;
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
            public Vector3 distance;
            public NativeArray<float3> Positions;
            public NativeArray<Matrix4x4> Result;
            public void Execute(int index)
            {
                Result[index] = Matrix4x4.TRS(Positions[index], Quaternion.identity, distance);
            }
        } 
        #endregion
    }
}