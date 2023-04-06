using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace JH.Portfolio.Map
{
    public class Map : MonoBehaviour
    {
        #region define 
        const int CAN_VISIT_TILE = (int)(Tile.Ground | Tile.Water);
        
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
        
        [ContextMenu("Initailized Map")]
        public void Init()
        {
            if(mapData == null)
                mapData = new MapData();
        }
        [ContextMenu("Clear Map")]
        public void Clear()
        {
            // tiles를 초기화한다.
            mapData.Clear();
            mapData = null;
        }

        public Vector3 GetWorldPosition(int x, int y)
        {
            var halfX = mapData.sizeX / 2;
            var halfY = mapData.sizeX / 2;
            return transform.position + new Vector3((x - halfX) * distance.x, 0, (y - halfY) * distance.z);
        }
        public (int, int) GetMapPosition(Vector3 worldPosition)
        {
            var halfX = mapData.sizeX / 2;
            var halfY = mapData.sizeX / 2;
            var position = transform.position;
            
            return new(
                Mathf.RoundToInt((worldPosition.x - position.x) / distance.x + halfX), 
                Mathf.RoundToInt((worldPosition.z - position.z) / distance.z + halfY)
                );
        }
        
        public bool OnMap(Vector3 worldPosition)
        {
            var (x, y) = GetMapPosition(worldPosition);
            return OnMap(x, y);
        }
        public bool OnMap(int x, int y)
        {
            return x >= 0 && x < mapData.sizeX && y >= 0 && y < mapData.sizeY;
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
        [HideInInspector, SerializeField] private bool onVisible = false;
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
            Vector3 center = Vector3.one * .1f;
            // draw grid
            for (int x = 0; x < mapData.sizeX; x++)
            {
                for (int y = 0; y < mapData.sizeY; y++)
                {
                    var pos = GetWorldPosition(x, y);
                    var currentColor = Gizmos.color;
                    switch (this[x, y])
                    {
                        case Tile.Ground:
                            Gizmos.color = Color.green;
                            break;
                        case Tile.Water:
                            Gizmos.color = Color.blue;
                            break;
                        case Tile.Build:
                            Gizmos.color = Color.red;
                            break;
                        case Tile.DeepWater:
                            Gizmos.color = Color.cyan;
                            break;
                    }
                    Gizmos.DrawCube(pos, center);
                    Gizmos.DrawWireCube(pos, distance);
                    Gizmos.color = currentColor;
                }
            } 
        }
        
        #endregion
    }
}