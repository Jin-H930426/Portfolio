using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace JH.Portfolio.Map
{
    [System.Serializable]
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
    public class MapData
    {
        public int sizeX;
        public int sizeY;
        public Tile[] tiles;
        public int[] tileCosts;

        public void Update(int sizeX, int sizeY)
        {
            var currentX = this.sizeX;
            var currentY = this.sizeY;
            var currentTiles = this.tiles;

            this.sizeX = sizeX;
            this.sizeY = sizeY;
            tiles = new Tile[sizeX * sizeY];
            tileCosts = new int[sizeX * sizeY];

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

            tileCosts = new int[sizeX * sizeY];
        }

        public void UpdateTileCost(in Dictionary<Tile, int> tileCost, int filterLenght)
        {
            int enumSize = Enum.GetValues(typeof(Tile)).Cast<int>().Last() + 1;
            int[] costArray = new int[enumSize];
            
            foreach (var pair in tileCost)
            {
                costArray[(int)pair.Key] = pair.Value;
            }
            
            NativeArray<int> cost = new NativeArray<int>(costArray, Allocator.TempJob);
            NativeArray<Tile> tileMap = new NativeArray<Tile>(tiles, Allocator.TempJob);
            NativeArray<int> costMap = new NativeArray<int>(tileCosts, Allocator.TempJob);

            var setTileCostJob = new SetTileCostJob()
            {
                costs = cost,
                tiles = tileMap,
                tileCosts = costMap,
            };
            var setTileCostJobHandle = setTileCostJob.Schedule();
            setTileCostJobHandle.Complete();
            
            this.tileCosts = costMap.ToArray();

            tileMap.Dispose();
            costMap.Dispose();
            cost.Dispose();
        }

        public void Clear()
        {
            sizeX = 0;
            sizeY = 0;
            tiles = null;
            tileCosts = null;
        }
    }

    public struct Line
    {
        const float VerticalLineGradient = 1e5f;
        float gradient;
        float interceptY;
        float2 pointOnLine1;
        float2 pointOnLine2;
        
        float gradientPerpendicular;
        bool approachSide;
        
        public Line(float2 pointOnLine, float2 pointPerpendicularToLine)
        {
            var dx = pointOnLine.x - pointPerpendicularToLine.x;
            var dy = pointOnLine.y - pointPerpendicularToLine.y;
            
            gradientPerpendicular = dx == 0 ? VerticalLineGradient : dy / dx;
            gradient = gradientPerpendicular == 0 ? VerticalLineGradient : -1 / gradientPerpendicular;
            interceptY = pointOnLine.y - gradient * pointOnLine.x;
            
            pointOnLine1 = pointOnLine;
            pointOnLine2 = pointOnLine + new float2(1, gradient);

            approachSide = false;
            approachSide = GetSide(pointPerpendicularToLine);
        }

        bool GetSide(float2 point)
        {
            return (point.x - pointOnLine1.x) * (pointOnLine2.y - pointOnLine1.y) > 
                   (point.y - pointOnLine1.y) * (pointOnLine2.x - pointOnLine1.x);
        }
        public bool HasCrossedLine(float2 point)
        {
            return GetSide(point) != approachSide;
        }
        
        public float DistanceFromPoint(float2 point)
        {
            var interceptPerpendicularY = point.y - gradientPerpendicular * point.x;
            var intersectX = (interceptPerpendicularY - interceptY) / (gradient - gradientPerpendicular);
            var intersectY = gradient * intersectX + interceptY;
            return math.distance(point, new float2(intersectX, intersectY));
        }
        
        #region Debug in Editor 
        #if UNITY_EDITOR
        public void DrawWithGizmos(float length)
        {
            var lineDir = new Vector3(1, 0, gradient).normalized;
            var lineCenter = new Vector3(pointOnLine1.x, 1, pointOnLine1.y);
            
            Gizmos.DrawLine(lineCenter - lineDir * length / 2f, lineCenter + lineDir * length / 2f);
        } 
        #endif
        #endregion
        
    }

    [System.Serializable]
    public class Path
    {
        public readonly float3[] lookPoints;
        public readonly Line[] turnBoundaries;
        public readonly int finishLineIndex;
        public readonly int slowDownIndex;

        public float this[float t] => 0;
        
        public Path(float3[] waypoints, float3 startPos, float turnDst, float stoppingDst)
        {
            // Path array
            lookPoints = waypoints;
            // boundary array
            turnBoundaries = new Line[lookPoints.Length];
            // finish line index
            finishLineIndex = turnBoundaries.Length - 1;
            // get previous point for calculating boundary
            // first line is from start position to first waypoint
            Vector2 previousPoint = ConvertVector(startPos);
            for (int i = 0; i < lookPoints.Length; i++)
            {
                // get current point
                Vector2 currentPoint = ConvertVector(lookPoints[i]);
                Vector2 dirToCurrentPoint = (currentPoint - previousPoint).normalized;
                var dst = Vector2.Distance(currentPoint, previousPoint);
                var minTurnDst = math.min(dst, turnDst);
                Vector2 turnBoundaryPoint = (i == finishLineIndex) ? 
                    currentPoint : currentPoint - dirToCurrentPoint * minTurnDst;
                
                turnBoundaries[i] = new Line(turnBoundaryPoint, previousPoint - dirToCurrentPoint * turnDst);
                // set current point as previous point for next iteration
                previousPoint = turnBoundaryPoint;
            }
            
            float dstFromEndPoint = 0;
            for (int i = lookPoints.Length - 1; i > 0; i--)
            {
                dstFromEndPoint += Vector3.Distance(lookPoints[i], lookPoints[i - 1]);
                if (dstFromEndPoint > stoppingDst)
                {
                    slowDownIndex = i;
                    break;
                }
            }
        }
        
        float2 ConvertVector(float3 v3)
        {
            return new float2(v3.x, v3.z);
        }

        #region Debug in Editor
        #if UNITY_EDITOR
        public void DrawWithGizmos()
        {
            if (lookPoints == null || turnBoundaries == null) return;
            Gizmos.color = Color.black;
            foreach (var p in lookPoints)
            {
                Gizmos.DrawCube(new Vector3(p.x, p.y + 1, p.z),Vector3.one * .1f);
            }
            Gizmos.color = Color.white;
            foreach (var l in turnBoundaries)
            {
                l.DrawWithGizmos(.5f);
            }
        }
        #endif
        #endregion
    }
    #region Job
    [BurstCompile]
    public struct SetTileCostJob : IJob
    {
        public int filterSize;
        public int SizeX;
        public int SizeY;
        public int Min;
        public int Max;
        
        public NativeArray<int> costs;
        [ReadOnly] public NativeArray<Tile> tiles;
        public NativeArray<int> tileCosts;

        public void Execute()
        {
            Min = int.MaxValue;
            Max = int.MinValue;
            
            int len = tiles.Length;
            for (var i = 0; i < len; i++)
            {
                tileCosts[i] = costs[(int)tiles[i]];
            }
            
            int filterExtent = filterSize / 2 + 1;
            
            NativeArray<int> horizontalPass = new NativeArray<int>(len, Allocator.Temp);
            NativeArray<int> verticalPass = new NativeArray<int>(len, Allocator.Temp);
            
            // calculate horizontal pass
            for (int y = 0; y < SizeY; y++)
            {
                int posY = y * SizeX;
                // x = 0 일 경우
                for (int x = -filterExtent; x <= filterExtent; x++)
                {
                    var sampleX = math.clamp(x, 0, filterExtent);
                    horizontalPass[posY * SizeX] += tileCosts[posY * SizeX + sampleX];
                }
                // x > 0 경우
                for (int x = 1; x < SizeX; x++)
                {
                    var removeX = math.clamp(x - filterExtent - 1, 0, SizeX);
                    var addX = math.clamp(x + filterExtent, 0, SizeX - 1);
                    horizontalPass[posY + x] = horizontalPass[posY + x - 1]
                        - tileCosts[posY + removeX] + tileCosts[posY + addX];
                }
            }
            // calculate vertical pass
            for (int x = 0; x < SizeX; x++)
            {
                // y = 0 일 경우
                for (int y = -filterExtent; y <= filterExtent; y++)
                {
                    var sampleY = math.clamp(y, 0, filterExtent);
                    verticalPass[x] += horizontalPass[sampleY * SizeX + x];
                }
                // y > 0 경우
                for (int y = 1; y < SizeY; y++)
                {
                    var removeY = math.clamp(y - filterExtent - 1, 0, SizeY);
                    var addY = math.clamp(y + filterExtent, 0, SizeY - 1);
                    
                    verticalPass[y * SizeX + x] = verticalPass[(y - 1) * SizeX + x]
                        - horizontalPass[removeY * SizeX + x] + horizontalPass[addY * SizeX + x];
                    
                    var cost = (int)math.round((float)verticalPass[y * SizeX + x] / (filterSize * filterSize));
                    tileCosts[y * SizeX + x] = cost;
                    
                    if (cost > Max)
                        Max = cost;
                    else if (cost < Min)
                        Min = cost;
                }
            }
        }
    }
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