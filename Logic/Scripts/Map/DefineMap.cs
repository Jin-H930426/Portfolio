using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace JH.Portfolio.Map
{
    [System.Serializable, Flags]
    public enum Tile
    {
        None = 0,
        Ground = 1,
        Grass = 2,
        Water = 4,
        Build = 8,
        DeepWater = 16,
    }

    [System.Serializable]
    public class Node : IHeapItem<Node>
    {
        public Tile tile;
        
        public int2 gridPosition;
        public float3 worldPosition;
        public int gCost { get; private set; } = 0;
        public int hCost { get; set; }
        public int fCost => gCost + hCost;
        public Node parent { get; private set; }

        public byte cost;
        public bool isWalkable => cost < byte.MaxValue;
        
        public Node(int2 gridPosition, float3 worldPosition, Tile tile, byte cost)
        {
            this.gridPosition = gridPosition;
            this.worldPosition = worldPosition;
            this.tile = tile;
            this.cost = cost;
            this.gCost = int.MaxValue;
        }
        public void SetParent(Node parent, int heuristic)
        {
            gCost = parent.gCost + heuristic;
            this.parent = parent;
        }
        public int CompareTo(Node other)
        {
            return fCost - other.fCost;
        }
        public int HeapIndex { get; set; }
    }
    public class MapData
    {
        public Node[] grid { get; private set; }
        public Node this[int x, int y]
        {
            get => grid[x + y * gridSize.x];
            private set => grid[x + y * gridSize.x] = value;
        }
        public Node this[int2 gridPosition]
        {
            get => grid[gridPosition.x + gridPosition.y * gridSize.x];
            private set => grid[gridPosition.x + gridPosition.y * gridSize.x] = value;
        }
        
        public int2 gridSize { get; private set; }
        public float3 cellSize { get; private set; }
        
        private float3 centerPoint;
        private float3 gridHalfSize;
        
        public MapData(int2 gridSize, float3 cellSize, float3 center, int scanningLayer ,SerializedDictionary<Tile, int> costs)
        {
            this.gridSize = gridSize;
            this.cellSize = cellSize;
            this.centerPoint = center;
            this.gridHalfSize = new float3(gridSize.x * cellSize.x, 0, gridSize.y * cellSize.z);
            
            CreateGrid(in scanningLayer, in costs);
        }
        void CreateGrid(in int scanningLayer, in SerializedDictionary<Tile, int> costs)
        {
            grid = new Node[gridSize.x * gridSize.y];
            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    var worldPosition =  centerPoint - gridHalfSize 
                                         + new float3(x * cellSize.x, 0, y * cellSize.z);
                    
                    var cell = new Node(new int2(x, y), worldPosition, Tile.Ground, (byte)costs[Tile.Ground]);
                    if (Physics.OverlapBox(worldPosition, cellSize * .5f,
                            Quaternion.identity, scanningLayer) is Collider[] colliders)
                    {
                        foreach (var collider in colliders)
                        {
                            var tile = Tile.Ground;
                            Enum.TryParse(collider.tag, out tile);
                            var cost = costs[tile];
                            if (cost >= 255)
                            {
                                cell.tile = tile;
                                cell.cost = 255;
                                break;
                            }

                            cell.tile |= tile;
                            cell.cost = (byte)((cell.cost + cost) / 2);
                        }
                    }

                    this[x, y] = cell;
                }
            }
        }

        public int2 GetGridPosition(float3 worldPosition)
        {
            var localPosition = worldPosition - centerPoint + gridHalfSize;
            var x = Mathf.Clamp((int) math.round(localPosition.x / cellSize.x), 0, gridSize.x - 1);
            var y = Mathf.Clamp((int) math.round(localPosition.z / cellSize.z), 0, gridSize.y - 1);
            return new int2(x, y);
        }
        public Node GetNodeFromWorldPos(float3 worldPosition)
        {
            return this[GetGridPosition(worldPosition)];
        }
        public bool IsInGrid(int2 gridPosition)
        {
            return IsInGrid(gridPosition.x, gridPosition.y);
        }
        public bool IsInGrid(int x, int y)
        {
            return x >= 0 && x < gridSize.x &&
                   y >= 0 && y < gridSize.y;
        }
        public bool IsVisible(int2 gridPosition)
        {
            return IsInGrid(gridPosition) && this[gridPosition].isWalkable;
        }
    }
    public class Heap<T> where T : IHeapItem<T>, IComparable<T>
    {
        T[] _items;
        HashSet<T> _hashSet;
        int _currentItemCount;
        public int Count => _currentItemCount;
        /// <summary>
        /// heap is making for priority queue
        /// </summary>
        /// <param name="heapSize">create HeapSize</param>
        public Heap(int heapSize)
        {
            _items = new T[heapSize];
            _hashSet = new HashSet<T>();
        }
        public void Clear()
        {
            _hashSet.Clear();
            _currentItemCount = 0;
        }
        /// <summary>
        /// push item to heap
        /// </summary>
        /// <param name="item">Addition item</param>
        /// <returns>result of addition</returns>
        public bool Push(T item)
        {
            if (!_hashSet.Add(item)) return false;
            if (_currentItemCount >= _items.Length)
            {
                var newItems = new T[_items.Length * 2];
                Array.Copy(_items, newItems, _items.Length);
                _items = newItems;
            }
            
            item.HeapIndex = _currentItemCount;
            _items[_currentItemCount] = item;
            SortUp(item);
            _currentItemCount++;

            return true;
        }
        void SortUp(T item)
        {
            int parentIndex = (item.HeapIndex - 1) / 2;
            while (true)
            {
                T parentItem = _items[parentIndex];
                if (item.CompareTo(parentItem) > 0)
                {
                    Swap(item, parentItem);
                }
                else
                {
                    break;
                }
                parentIndex = (item.HeapIndex - 1) / 2;
            }
        }
        /// <summary>
        /// pop first item
        /// </summary>
        /// <returns>first heap item</returns>
        public T Pop()
        {
            T firstItem = _items[0];
            _hashSet.Remove(firstItem);
            _currentItemCount--;
            _items[0] = _items[_currentItemCount];
            _items[0].HeapIndex = 0;
            SortDown(_items[0]);
            return firstItem;
        }
        /// <summary>
        /// if you want to pop item,
        /// you should call this method
        /// </summary>
        /// <param name="item">item of release</param>
        /// <returns></returns> 
        void SortDown(T item)
        {
            while (true)
            {
                // get child index
                var childIndexLeft = item.HeapIndex * 2 + 1;
                var childIndexRight = item.HeapIndex * 2 + 2;
                var swapIndex = 0;
                // check if child exist
                // if left chile exist, always right child exist
                if (childIndexLeft < _currentItemCount)
                {
                    swapIndex = childIndexLeft;
                    if (childIndexRight < _currentItemCount)
                    {
                        if (_items[childIndexLeft].CompareTo(_items[childIndexRight]) < 0)
                        {
                            swapIndex = childIndexRight;
                        }
                    }
                    if (item.CompareTo(_items[swapIndex]) < 0)
                    {
                        Swap(item, _items[swapIndex]);
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
        }
        /// <summary>
        /// update item
        /// </summary>
        /// <param name="item"></param>
        public void UpdateItem(T item)
        {
            SortUp(item);
        }
        /// <summary>
        /// Check if item exist in heap
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(T item)
        {
            return _hashSet.Contains(item);
        }
        /// <summary>
        /// Swap heap iteam
        /// </summary>
        /// <param name="itemA"></param>
        /// <param name="itemB"></param>
        void Swap(T itemA, T itemB)
        {
            _items[itemA.HeapIndex] = itemB;
            _items[itemB.HeapIndex] = itemA;
            
            int itemAIndex = itemA.HeapIndex;
            itemA.HeapIndex = itemB.HeapIndex;
            itemB.HeapIndex = itemAIndex;
        }
        
        public T[] GetItems()
        {
            return _items;
        }
        public IEnumerable<T> GetEnumerable()
        {
            for (int i = 0; i < _currentItemCount; i++)
            {
                yield return _items[i];
            }
        }
    }
    public interface IHeapItem<T> : IComparable<T>
    {
        int HeapIndex { get; set; }
    }
}