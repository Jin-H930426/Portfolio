using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace JH.Portfolio.Map.JSP
{
    [SerializeField]
    public class JpsMapBaker : MonoBehaviour
    {
        const int MOVE_STRAIGHT_COST = 10;
        const int MOVE_DIAGONAL_COST = 14;
        
        [Header("For Scanning Map")]
        public LayerMask scanningLayers;

        public SerializedDictionary<Tile, int> costs;
        private MapData _mapData;
        
        public int2 gridSize = 10;
        public float3 cellSize = 1;
        
        public void Bake()
        {
            _mapData = new MapData(gridSize, cellSize, transform.position, scanningLayers, costs );
        }

        public void PathFinding(float3 startPos, float3 endPos)
        {
            
            var openSet = new Heap<Node>(_mapData.grid.Length);
            var closeSet = new HashSet<int2>();
            
            Node startNode = _mapData.GetNodeFromWorldPos(startPos);
            Node endNode = _mapData.GetNodeFromWorldPos(endPos);
            
            
            startNode.hCost = Heuristic(startNode, endNode);
            openSet.Push(startNode);

            while (openSet.Count > 0)
            {
                SearchRight(startNode, endNode, PushNode, CheckClose, RemoveClose);
            }
            
            void PushNode(Node prev, Node current)
            {
                current.SetParent(prev, Heuristic(prev, current));
                current.hCost = Heuristic(current, endNode);
                openSet.Push(current);
            }
            bool CheckClose(int2 gridPos)
            {
                return closeSet.Contains(gridPos);
            }
            bool RemoveClose(int2 gridPos)
            {
                return closeSet.Remove(gridPos);
            }
        }

        private bool SearchRight(Node startNode, Node endNode, 
            Action<Node, Node> pushNode, Func<int2, bool> checkClose, Func<int2, bool> removeClose)
        {
            var x = startNode.gridPosition.x;
            var y = startNode.gridPosition.y;
            
            Node prevNode = startNode;
            Node currentNode = startNode;
            
            bool isFind = false;
            while(currentNode.isWalkable)
            {
                // check next node is in grid
                if (_mapData.IsInGrid(++x, y)) break;
                
                // move to next node
                currentNode = _mapData[x, y];
                // check next node is in close set
                // or next node is not walkable
                if (checkClose(currentNode.gridPosition) || !currentNode.isWalkable) break;
                
                // Check next node is end node
                if (currentNode == endNode)
                {
                    isFind = true;
                    pushNode(currentNode, prevNode);
                    break;
                }

                if (SearchRight_CloseUpOpenRightUp(prevNode, currentNode, pushNode, checkClose))
                {
                    isFind = true;
                    break;
                }

                if (SearchRight_CloseUpOpenRightDown(prevNode, currentNode, pushNode, checkClose))
                {
                    isFind = true;
                    break;
                }
            }
            removeClose(currentNode.gridPosition);
            return isFind;
        }
        bool SearchRight_CloseUpOpenRightUp(Node perv, Node current, Action<Node, Node> pushNode, Func<int2, bool>checkClose)
        {
            var x = current.gridPosition.x;
            var y = current.gridPosition.y;
            
            if (!_mapData.IsInGrid(x + 1, y + 1)) return false; // if right up node is not in grid, return false 
            if (_mapData[x, y + 1].isWalkable) return false; // if up node is walkable, return false 
            pushNode(perv, current);
            return true;
        }
        bool SearchRight_CloseUpOpenRightDown(Node perv, Node current, Action<Node, Node> pushNode, Func<int2, bool>checkClose)
        {
            var x = current.gridPosition.x;
            var y = current.gridPosition.y;

            if (!_mapData.IsInGrid(x + 1, y)) return false;
            if (!_mapData[x, y - 1].isWalkable) return false;
            pushNode(perv, current);
            return true;
        }

        private bool SearchLeft(Node startNode, Node endNode,
            Action<Node, Node> pushNode, Func<int2, bool> checkClose, Func<int2, bool> removeClose)
        {
            var x = startNode.gridPosition.x;
            var y = startNode.gridPosition.y;
            
            Node prevNode = startNode;
            Node currentNode = startNode;
            
            bool isFind = false;
            while(currentNode.isWalkable)
            {
                // check next node is in grid
                if (_mapData.IsInGrid(--x, y))
                {
                    break;
                }
                // move to next node
                currentNode = _mapData[x, y];
                // check next node is in close set
                // or next node is not walkable
                if (checkClose(currentNode.gridPosition) || !currentNode.isWalkable)
                {
                    break;
                }
                // Check next node is end node
                if (currentNode == endNode)
                {
                    isFind = true;
                    pushNode(currentNode, prevNode);
                    break;
                }

                if (SearchLeft_CloseUpOpenLeftUp(prevNode, currentNode, pushNode, checkClose))
                {
                    isFind = true;
                    break;
                }

                if (SearchLeft_CloseUpOpenLeftDown(prevNode, currentNode, pushNode, checkClose))
                {
                    isFind = true;
                    break;
                }
            }
            removeClose(currentNode.gridPosition);
            return isFind;
        }
        bool SearchLeft_CloseUpOpenLeftUp(Node perv, Node current, Action<Node, Node> pushNode, Func<int2, bool>checkClose)
        {
            var x = current.gridPosition.x;
            var y = current.gridPosition.y;
            
            if (!_mapData.IsInGrid(x + 1, y + 1)) return false; // if right up node is not in grid, return false 
            if (_mapData[x, y + 1].isWalkable) return false; // if up node is walkable, return false 
            pushNode(perv, current);
            return true;
        }

        bool SearchLeft_CloseUpOpenLeftDown(Node perv, Node current, Action<Node, Node> pushNode,
            Func<int2, bool> checkClose)
        {
            var x = current.gridPosition.x;
            var y = current.gridPosition.y;

            if (!_mapData.IsInGrid(x + 1, y)) return false;
            if (!_mapData[x, y - 1].isWalkable) return false;
            pushNode(perv, current);
            return true;
        }

        public int Heuristic(Node a, Node b)
        {
            int x = math.abs(a.gridPosition.x - b.gridPosition.x);
            int y = math.abs(a.gridPosition.y - b.gridPosition.y);
            return MOVE_DIAGONAL_COST * math.min(x, y) + MOVE_STRAIGHT_COST * math.abs(x - y);
        }
    }
}