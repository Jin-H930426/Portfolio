using System;
using Unity.Mathematics;

namespace JH.Portfolio.Map
{
    public class AstarNode : IComparable<AstarNode>
    {
        public bool isWalkable;
        public int x;
        public int y;
        public int2 pos => new int2(x, y);
        public int gCost;
        public int hCost;
        public int fCost => gCost + hCost;
        
        public AstarNode parent;
        
        public static AstarNode Create(int x, int y, bool isWalkable)
        {
            return new AstarNode()
            {
                isWalkable = isWalkable,
                x = x,
                y = y,
            };
        }
        public static AstarNode Create(int2 pos, bool isWalkable)
        {
            return new AstarNode()
            {
                isWalkable = isWalkable,
                x = pos.x,
                y = pos.y,
            };
        }
        
        public int CompareTo(AstarNode other)
        {
            return x - other.x + y - other.y;
        }
        
        // operator ==, !=
        public static bool operator ==(AstarNode a, AstarNode b)
        {
            return a.x == b.x && a.y == b.y;
        }
        public static bool operator !=(AstarNode a, AstarNode b)
        {
            return a.x != b.x || a.y != b.y;
        }
        
        // operate <. >, <=, >=
        public static bool operator <(AstarNode a, AstarNode b)
        {
            return a.fCost < b.fCost;
        }
        public static bool operator >(AstarNode a, AstarNode b)
        {
            return a.fCost > b.fCost;
        }
        public static bool operator <=(AstarNode a, AstarNode b)
        {
            return a.fCost <= b.fCost;
        }
        public static bool operator >=(AstarNode a, AstarNode b)
        {
            return a.fCost >= b.fCost;
        }
        
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
    public class Astat
    {
        
    }
}