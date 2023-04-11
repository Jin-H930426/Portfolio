using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using Unity.Mathematics;

namespace JH.Portfolio.Map.Astar
{
    public class Heap<T> where T : IHeapItem<T>, IComparable<T>
    {
        T[] _items;
        HashSet<T> _hashSet;
        int _currentItemCount;
        public int Count => _currentItemCount;
        
        public Heap(int maxHeapSize)
        {
            _items = new T[maxHeapSize];
            _hashSet = new HashSet<T>();
        }
        public void Clear()
        {
            _hashSet.Clear();
            _currentItemCount = 0;
        }
        
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
        
        public void UpdateItem(T item)
        {
            SortUp(item);
        }
        public bool Contains(T item)
        {
            return _hashSet.Contains(item);
        }
        
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
    public class AstarNode : IHeapItem<AstarNode>
    {
        public int x, y;
        public int gCost, hCost;
        public int tileCost;
        
        
        public int2 Pos => new int2(x, y);
        public int fCost => gCost + hCost;
        public int HeapIndex { get; set; }
        
        public AstarNode parent = null;

        public static AstarNode Create(int x, int y, int tileCost)
        {
            return new AstarNode()
            {
                x = x,
                y = y,
                tileCost = tileCost
            };
        }
        public static AstarNode Create(int2 pos, int tileCost)
        {
            return Create(pos.x, pos.y, tileCost);
        }
        
        public int CompareTo(AstarNode other)
        {
            int compare = fCost.CompareTo(other.fCost);
            if (compare == 0)
            {
                compare = hCost.CompareTo(other.hCost);
            }
            return -compare;
        }
        
        // operator ==, !=
        public static bool operator ==(AstarNode a, AstarNode b) => a.x == b.x && a.y == b.y;
        public static bool operator !=(AstarNode a, AstarNode b) => a.x != b.x || a.y != b.y;
        
        // operate <. >, <=, >=
        public static bool operator <(AstarNode a, AstarNode b) => a.fCost < b.fCost;
        public static bool operator >(AstarNode a, AstarNode b) => a.fCost > b.fCost;
        public static bool operator <=(AstarNode a, AstarNode b) => a.fCost <= b.fCost;
        public static bool operator >=(AstarNode a, AstarNode b) => a.fCost >= b.fCost;
        
        public static int GetDistance(AstarNode a, AstarNode b)
        {
            var disX = math.abs(a.x - b.x);
            var disY = math.abs(a.y - b.y);
            if (disX > disY)
                return 14 * disY + 10 * (disX - disY);
            else
                return 14 * disX + 10 * (disY - disX);
        }
    }
}