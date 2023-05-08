using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using Unity.Mathematics;

namespace JH.Portfolio.Map.Astar
{
    
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
        public static bool operator ==(AstarNode a, AstarNode b) =>  a.x == b.x && a.y == b.y;
        public static bool operator !=(AstarNode a, AstarNode b) =>  a.x != b.x || a.y != b.y;
        
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