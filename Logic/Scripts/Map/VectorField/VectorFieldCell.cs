using Unity.Mathematics;
namespace JH.Portfolio.Map
{
    public class VectorFieldCell
    {
        public float3 worldPosition;
        public int2 gridPosition;
        public byte cost;
        public ushort bestCost;
        public GridDirection bestDirection;
        
        public VectorFieldCell(float3 worldPosition, int2 gridPosition)
        {
            this.worldPosition = worldPosition;
            this.gridPosition = gridPosition;
            cost = 1;
            bestCost = ushort.MaxValue;
            bestDirection = GridDirection.None;
        }
        public void IncreaseCost(int amount)
        {
            if (cost == byte.MaxValue) return;
            if (amount + cost >= 255)
            {
                cost = byte.MaxValue; 
                return;
            }
            
            cost += (byte) amount;
        }
    }
}