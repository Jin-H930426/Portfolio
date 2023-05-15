using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace JH.Portfolio.Map
{
    public class VectorField
    {
        public VectorFieldCell[,] grid { get; private set; }
        public int2 gridSize { get; private set; }
        public float cellRadius { get; private set; }
        private VectorFieldCell _destinationVectorFieldCell;

        private float cellDiameter;
        private float3 centerPoint;
        private float3 gridHalfSize;

        public VectorField(float cellRadius, int2 gridSize, float3 center)
        {
            this.cellRadius = cellRadius;
            this.gridSize = gridSize;
            this.cellDiameter = cellRadius * 2;
            
            centerPoint = center;
            gridHalfSize = new float3(gridSize.x * cellDiameter * .5f, 0, gridSize.y * cellDiameter * .5f);
            
            CreateGrid();
        }

        public void CreateGrid()
        {
            grid = new VectorFieldCell[gridSize.x, gridSize.y];
            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    var worldPosition =  centerPoint - gridHalfSize + new float3(x * cellDiameter + cellRadius, 
                        0, y * cellDiameter + cellRadius);
                    grid[x, y] = new VectorFieldCell(worldPosition, new int2(x, y));
                }
            }
        }

        public void CreateField()
        {
            foreach (var cell in grid)
            {
               GetNeighborCells(cell.gridPosition, GridDirection.AllDirections, out var neighbors); 
               int bestCost = cell.bestCost;
                foreach (var neighbor in neighbors)
                {
                    if (neighbor.bestCost < bestCost)
                    {
                        bestCost = neighbor.bestCost;
                        cell.bestDirection = GridDirection.GetDirectionFromVector(neighbor.gridPosition - cell.gridPosition);
                    }
                }
            }
        }
        
        public void CreateCostField(int layerMask, in SerializedDictionary<Tile, int> costs)
        {
            float3 cellHalfExtents = 1 * cellRadius;

            foreach (var cell in grid)
            {
                var obstacles = Physics.OverlapBox(cell.worldPosition, cellHalfExtents,
                    Quaternion.identity, layerMask);
                foreach (var obstacle in obstacles)
                {
                    var cost = costs[Enum.Parse<Tile>(obstacle.tag)];
                    cell.IncreaseCost(cost);
                    
                    if (cell.cost == byte.MaxValue) break;
                }
            }
        }
        public void CreateIntegrationField(VectorFieldCell destinationVectorFieldCell)
        {
            this._destinationVectorFieldCell = destinationVectorFieldCell;
            this._destinationVectorFieldCell.bestCost = 0;
            this._destinationVectorFieldCell.bestCost = 0;
            UnityEngine.Debug.Log($"destinationCell: {destinationVectorFieldCell.gridPosition}");
            
            Queue<VectorFieldCell> cellsToCheck = new Queue<VectorFieldCell>();
            
            cellsToCheck.Enqueue(this._destinationVectorFieldCell);
            while (cellsToCheck.Count > 0)
            {
                var cell = cellsToCheck.Dequeue();
                GetNeighborCells(cell.gridPosition, GridDirection.CardinalDirections, out var neighbors);
                
                foreach (var neighbor in neighbors)
                {
                    if (neighbor.cost == byte.MaxValue) continue;
                    if(neighbor.cost + cell.bestCost < neighbor.bestCost)
                    {
                        neighbor.bestCost = (ushort)(neighbor.cost + cell.bestCost);
                        cellsToCheck.Enqueue(neighbor);
                    }
                }
            }
        }
        private void GetNeighborCells(int2 gridPosition, List<GridDirection> directions, out List<VectorFieldCell> neighbors)
        {
            neighbors = new List<VectorFieldCell>();
            foreach (var direction in directions)
            {
                if(GetCellAtRelativePos(gridPosition, direction) is VectorFieldCell newNeighbor)
                    neighbors.Add(newNeighbor);
            }
        }
        
        private VectorFieldCell GetCellAtRelativePos(int2 orignPos, int2 relativePos)
        {
            var cellPos = orignPos + relativePos;
            
            if (cellPos.x < 0 || cellPos.x >= gridSize.x || cellPos.y < 0 || cellPos.y >= gridSize.y) 
                return null;
            
            return grid[cellPos.x, cellPos.y];
        }
        public VectorFieldCell GetCellFromWorldPos(float3 worldPos)
        {
            worldPos -= centerPoint - gridHalfSize;
            
            float percentX = math.clamp(worldPos.x / (gridSize.x * cellDiameter), 0, 1);
            float percentY = math.clamp(worldPos.z / (gridSize.y * cellDiameter), 0 , 1);
            
            int x = math.clamp((int)math.floor(gridSize.x*percentY), 0, gridSize.x - 1);
            int y = math.clamp((int)math.floor(gridSize.y*percentX), 0, gridSize.y - 1);
            return grid[x, y];
        }
    }
}