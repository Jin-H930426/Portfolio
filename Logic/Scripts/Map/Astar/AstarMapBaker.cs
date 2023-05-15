using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Mathematics;

namespace JH.Portfolio.Map
{
    using Astar;

    public class AstarMapBaker : MonoBehaviour
    {
        #region define

        private static Dictionary<string, AstarMapBaker> _maps;
        const int CAN_VISIT_TILE = (int)(Tile.Ground | Tile.Water);

        readonly int2[] DIRECTION = new int2[]
        {
            new int2(0, 1), // 상
            new int2(1, 1),
            new int2(1, 0), // 우
            new int2(1, -1),
            new int2(0, -1), // 하
            new int2(-1, -1),
            new int2(-1, 0), // 좌
            new int2(-1, 1),
        };

        #endregion

        #region variable
        [SerializeField] private string mapName;
        [SerializeField] private int2 gridSize;
        [SerializeField] private int filterLenght = 3;

        [SerializeField] private SerializedDictionary<Tile, int> tileCosts
            = new SerializedDictionary<Tile, int>(Enum.GetValues(typeof(Tile)).Cast<Tile>().ToArray());

        [SerializeField] private Vector3 distance = Vector3.one;
        [SerializeField] private AstarMapData astarMapData;
        #endregion

        #region property

        public int SizeX => gridSize.x;
        public int SizeY => gridSize.y;
        public float GroundHeight => transform.position.y;

        public Vector3 Distance => distance;

        // operator for tiles
        public Tile this[int x, int y]
        {
            get => astarMapData != null ? astarMapData.tiles[y * astarMapData.sizeX + x] : Tile.None;
            set
            {
                if (astarMapData != null) astarMapData.tiles[y * astarMapData.sizeX + x] = value;
            }
        }

        public int GetTileCost(int x, int y)
        {
            return astarMapData != null ? astarMapData.tileCosts[y * astarMapData.sizeX + x] : 0;
        }

        #endregion

        void Awake()
        {
            if (_maps == null)
                _maps = new Dictionary<string, AstarMapBaker>();
            _maps[mapName] = this;
        }

        /// <summary>
        /// Get world position from x, y map vector
        /// </summary>
        /// <param name="x">map position x</param>
        /// <param name="y">map position y</param>
        /// <returns>world position</returns>
        public float3 GetWorldPosition(int x, int y)
        {
            float3 position = transform.position;
            var halfX = astarMapData.sizeX / 2;
            var halfY = astarMapData.sizeX / 2;
            return position + new float3((x - halfX) * distance.x, GroundHeight, (y - halfY) * distance.z);
        }

        /// <summary>
        /// Get world position from map position
        /// </summary>
        /// <param name="mapPosition">map position</param>
        /// <returns></returns>
        public float3 GetWorldPosition(int2 mapPosition)
        {
            return GetWorldPosition(mapPosition.x, mapPosition.y);
        }

        /// <summary>
        /// Get map position from world position
        /// </summary>
        /// <param name="worldPosition">world position</param>
        /// <returns>map position</returns>
        public int2 GetMapPosition(float3 worldPosition)
        {
            return GetMapPosition(worldPosition.x, worldPosition.y, worldPosition.z);
        }

        /// <summary>
        /// Get map position from world position
        /// </summary>
        /// <param name="x">world position x</param>
        /// <param name="y">world position y</param>
        /// <param name="z">world position z</param>
        /// <returns>map position</returns>
        public int2 GetMapPosition(float x, float y, float z)
        {
            var halfX = astarMapData.sizeX / 2;
            var halfY = astarMapData.sizeX / 2;
            var position = transform.position;

            return new(
                Mathf.RoundToInt((x - position.x) / distance.x + halfX),
                Mathf.RoundToInt((z - position.z) / distance.z + halfY)
            );
        }

        /// <summary>
        /// Check if world position is on map
        /// </summary>
        /// <param name="worldPosition">world position</param>
        /// <returns>state on the map</returns>
        public bool OnMap(float3 worldPosition)
        {
            if (astarMapData == null) return false;

            return OnMap(GetMapPosition(worldPosition));
        }

        /// <summary>
        /// Check if world position is on map
        /// </summary>
        /// <param name="x">world position x</param>
        /// <param name="y">world position y</param>
        /// <param name="z">world position z</param>
        /// <returns>state on the map</returns>
        public bool OnMap(float x, float y, float z)
        {
            if (astarMapData == null) return false;

            return OnMap(GetMapPosition(x, y, z));
        }

        /// <summary>
        /// Check if map position is on map
        /// </summary>
        /// <param name="mapPosition">map position</param>
        /// <returns>state on the map</returns>
        public bool OnMap(int2 mapPosition)
        {
            return OnMap(mapPosition.x, mapPosition.y);
        }

        /// <summary>
        /// Check if map position is on map
        /// </summary>
        /// <param name="x">map position x</param>
        /// <param name="y">map position y</param>
        /// <returns>state on the map</returns>
        public bool OnMap(int x, int y)
        {
            if (astarMapData == null) return false;

            return x >= 0 && x < astarMapData.sizeX && y >= 0 && y < astarMapData.sizeY;
        }

        /// <summary>
        /// Check possibility to visit tile
        /// </summary>
        /// <param name="mapPosition">map position</param>
        /// <returns>value of possibility</returns>
        public bool CanVisit(int2 mapPosition)
        {
            return CanVisit(mapPosition.x, mapPosition.y);
        }

        /// <summary>
        /// Check possibility to visit tile
        /// </summary>
        /// <param name="x">map position x</param>
        /// <param name="y">map position y</param>
        /// <returns></returns>
        public bool CanVisit(int x, int y)
        {
            if (!OnMap(x, y)) return false;
            return ((int)this[x, y] & CAN_VISIT_TILE) != 0;
        }

        /// <summary>
        /// Find map containing world position
        /// </summary>
        /// <param name="worldPosition">world position</param>
        /// <returns></returns>
        public static AstarMapBaker FindMapWith(float3 worldPosition)
        {
            return FindMapWith(worldPosition.x, worldPosition.y, worldPosition.z);
        }

        /// <summary>
        /// Find map containing world position
        /// </summary>
        /// <param name="x">world position x</param>
        /// <param name="y">world position y</param>
        /// <param name="z">world position z</param>
        /// <returns>map with world position</returns>
        public static AstarMapBaker FindMapWith(float x, float y, float z)
        {
            foreach (var map in _maps.Values)
            {
                if (map.OnMap(x, y, z))
                    return map;
            }

            return null;
        }

        #region Path finding

        AstarNode PathFindingWithAstar(int2 startPos, int2 endPos)
        {
            if (!CanVisit(startPos) || !CanVisit(endPos) || startPos.Equals(endPos))
            {
                return null;
            }

            // create start and end nodes
            AstarNode startNode = AstarNode.Create(startPos, tileCosts[this[startPos.x, startPos.y]]);
            AstarNode endNode = AstarNode.Create(endPos, tileCosts[this[endPos.x, endPos.y]]);
            // create open list and close list
            Heap<AstarNode> openList = new Heap<AstarNode>(100);
            HashSet<int2> closeSet = new HashSet<int2>();
            Dictionary<int2, AstarNode> openSet = new Dictionary<int2, AstarNode>(); // for fast search

            openList.Push(startNode);
            openSet.Add(startNode.Pos, startNode);
            while (openList.Count > 0 && // if open list is empty, stop path finding
                   openList.Count < 30000) // if path finding is too long, stop path finding
            {
                var currentNode = openList.Pop();
                openSet.Remove(currentNode.Pos);
                closeSet.Add(currentNode.Pos);
                if (currentNode == endNode)
                {
                    return currentNode;
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
            return null;
        }
        AstarNode PathFindingWithAstar4Dir(int2 startPos, int2 endPos)
        {
            if (!CanVisit(startPos) || !CanVisit(endPos) || startPos.Equals(endPos))
            {
                return null;
            }

            // create start and end nodes
            AstarNode startNode = AstarNode.Create(startPos, tileCosts[this[startPos.x, startPos.y]]);
            AstarNode endNode = AstarNode.Create(endPos, tileCosts[this[endPos.x, endPos.y]]);
            // create open list and close list
            Heap<AstarNode> openList = new Heap<AstarNode>(100);
            HashSet<int2> closeSet = new HashSet<int2>();
            Dictionary<int2, AstarNode> openSet = new Dictionary<int2, AstarNode>(); // for fast search

            openList.Push(startNode);
            openSet.Add(startNode.Pos, startNode);

            while (openList.Count > 0 && // if open list is empty, stop path finding
                   openList.Count < 30000) // if path finding is too long, stop path finding
            {
                var currentNode = openList.Pop();
                openSet.Remove(currentNode.Pos);

                closeSet.Add(currentNode.Pos);

                if (currentNode == endNode)
                {
                    return currentNode;
                }

                for (int i = 0; i < 8; i += 2)
                {
                    var dir = DIRECTION[i];
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

            return null;
        }

        /// <summary>
        /// Get path from start map position to end map position
        /// </summary>
        /// <param name="startPos">start map position</param>
        /// <param name="endPos">end map position</param>
        /// <param name="movePoints">out map point path</param>
        public void PathFindingWithAstar(int2 startPos, int2 endPos, out List<int2> movePoints)
        {
            if (PathFindingWithAstar(startPos, endPos) is not AstarNode currentNode)
            {
                movePoints = null;
                return;
            }

            movePoints = new List<int2>();
            while (true)
            {
                movePoints.Add(currentNode.Pos);
                if (currentNode.parent.Pos.Equals(startPos))
                    break;
                currentNode = currentNode.parent;
            }

            movePoints.Reverse();
        }

        /// <summary>
        /// Get path from start world position to end world position
        /// </summary>
        /// <param name="startPos">start world position</param>
        /// <param name="endPos">end world position</param>
        /// <param name="movePoints">out world point path</param>
        public void PathFindingWithAstar(float3 startPos, float3 endPos, out List<float3> movePoints)
        {
            var sp = GetMapPosition(startPos);
            var ep = GetMapPosition(endPos);
            if (PathFindingWithAstar(sp, ep) is not AstarNode currentNode)
            {
                movePoints = null;
                return;
            }
            movePoints = new List<float3>();
            endPos.y = GroundHeight;
            movePoints.Add(endPos);
            while (true)
            {
                movePoints.Add(GetWorldPosition(currentNode.Pos));
                if (currentNode.parent.Pos.Equals(sp))
                    break;
                currentNode = currentNode.parent;
            }
            movePoints.Add(startPos);
            movePoints.Reverse();
        }

        /// <summary>
        /// Get path from start map position to end map position
        /// just can move 4 direction
        /// up, down, left, right
        /// </summary>
        /// <param name="startPos">start map position</param>
        /// <param name="endPos">end map position</param>
        /// <param name="movePoints">out map point path</param>
        public void PathFindingWithAstar4Dir(int2 startPos, int2 endPos, out List<int2> movePoints)
        {
            if (PathFindingWithAstar4Dir(startPos, endPos) is not AstarNode currentNode)
            {
                movePoints = null;
                return;
            }

            movePoints = new List<int2>();
            while (true)
            {
                movePoints.Add(currentNode.Pos);
                if (currentNode.parent.Pos.Equals(startPos))
                    break;
                currentNode = currentNode.parent;
            }

            movePoints.Reverse();
        }

        /// <summary>
        /// Get path from start world position to end world position
        /// just can move 4 direction
        /// up, down, left, right
        /// </summary>
        /// <param name="startPos">start world position</param>
        /// <param name="endPos">end world position</param>
        /// <param name="movePoints">out world point path</param>
        public void PathFindingWithAstar4Dir(float3 startPos, float3 endPos, out List<float3> movePoints)
        {
            var sp = GetMapPosition(startPos);
            var ep = GetMapPosition(endPos);

            if (PathFindingWithAstar4Dir(sp, ep) is not AstarNode currentNode)
            {
                movePoints = null;
                return;
            }

            movePoints = new List<float3>();
            endPos.y = GroundHeight;
            movePoints.Add(endPos);
            while (true)
            {
                movePoints.Add(GetWorldPosition(currentNode.Pos));
                if (currentNode.parent.Pos.Equals(sp))
                    break;
                currentNode = currentNode.parent;
            }

            movePoints.Reverse();
        }

        #endregion

        #region EDITOR

#if UNITY_EDITOR
        // 초기 메쉬를 큐브로 설정
        [SerializeField] private Mesh mesh;
        [SerializeField] private bool onVisible = false;

        [SerializeField] private SerializedDictionary<Tile, Color> gridColor
            = new SerializedDictionary<Tile, Color>(Enum.GetValues(typeof(Tile)).Cast<Tile>().ToArray());

        [SerializeField] private Texture2D _texture2D;
        [SerializeField] private Material _textureMaterial;
        [SerializeField] private Matrix4x4 _textureMatrix;
        
        
        [ContextMenu("Bake Map")]
        public void BakeMap()
        {
            ClearMap();
            if (astarMapData == null)
                astarMapData = new AstarMapData();

            astarMapData.Update(SizeX, SizeY);
            SetMapData();
            GreateGridMap();
            astarMapData.UpdateTileCost(tileCosts, filterLenght);
        }
        [ContextMenu("Clear Map")]
        public void ClearMap()
        {
            if (!_texture2D) DestroyImmediate(_texture2D);
            _texture2D = null;

            astarMapData.Clear();
            astarMapData = null;
        }
        private void SetMapData()
        {
            for (var x = 0; x < astarMapData.sizeX; x++)
            {
                for (var y = 0; y < astarMapData.sizeY; y++)
                {
                    var worldPos = GetWorldPosition(x, y);

                    if (Physics.CheckBox(worldPos, Distance, Quaternion.identity,
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

        void GreateGridMap()
        {
            if (!_texture2D)
            {
                _texture2D = new Texture2D(astarMapData.sizeX, astarMapData.sizeY, TextureFormat.RGBAFloat, true);
                for (var x = 0; x < astarMapData.sizeX; x++)
                {
                    for (var y = 0; y < astarMapData.sizeY; y++)
                    {
                        _texture2D.SetPixel(x, y, gridColor[astarMapData.tiles[y * astarMapData.sizeX + x]]);
                    }
                }

                _texture2D.Apply();
                if (_textureMaterial == null)
                {
                    _textureMaterial = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default"));
                    Color c = Color.white;
                    c.a = 78.0f / 255.0f;
                    _textureMaterial.color = c;
                }

                _textureMaterial.mainTexture = _texture2D;
            }
        }
        public void DrawMap()
        {
            _textureMatrix = Matrix4x4.TRS(transform.position + Vector3.up * .2f, Quaternion.Euler(90, 0, 0),
                new Vector3(astarMapData.sizeX * distance.x, astarMapData.sizeY * distance.z, 1));
            Graphics.DrawMesh(mesh, _textureMatrix, _textureMaterial, 0);
        }

        private void OnValidate()
        {
            BakeMap();
        }

        private void OnDrawGizmosSelected()
        {
            DrawMap();
        }
#endif

        #endregion
    }
}