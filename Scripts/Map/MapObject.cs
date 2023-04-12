using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Mathematics;

namespace JH.Portfolio.Map
{
    public class MapObject : MonoBehaviour
    {
        private Map _map;
        [SerializeField, ReadOnly] Path _path;
        
        [ContextMenuItem("Calculate Path", "EditorCalculatePath")]
        [SerializeField] Transform _target;

        [SerializeField] private float _turnDst = 5;
        [SerializeField] private float _stoppingDst = 10;
        
        public Transform target { get => _target; }
        public float turnDst { get => _turnDst; set => _turnDst = value; }
        public float stoppingDst { get => _stoppingDst; set => _stoppingDst = value; }
        
        private void Start()
        {
            _map = Map.GetOnMap(transform.position);
        }

        public void CalculatePath(Vector3 endPos)
        {
            CalculatePath(transform.position, endPos);
        }
        public void CalculatePath(Vector3 startPos, Vector3 endPos)
        {
            _map.PathFindingWithAstar(startPos, endPos, out var wayPoint);
            if (wayPoint == null) return;
            var w = wayPoint.ToArray();
            _path = new Path(w, transform.position, _turnDst, _stoppingDst);
            wayPoint = null;
        }
        
        public IEnumerator<(float3 pos, quaternion rot)> CalculationMovement(float3 currentPos, 
            quaternion currentRot, float deltaTime, float movementSpeed ,float turnSpeed)
        {  
            bool followingPath = true;
            int pathIndex = 0;
            
            float speedPercent = 1;
            
            while (followingPath)
            {
                float2 pos2D = new float2(currentPos.x, currentPos.z);
                if (_path.turnBoundaries == null) break;
                if(_path.turnBoundaries.Length <= pathIndex)
                {
                    Debug.Log($"Path Index Out of Range: {pathIndex} / {_path.turnBoundaries.Length}");
                    break;
                }
                while (_path.turnBoundaries[pathIndex].HasCrossedLine(pos2D))
                {
                    if (pathIndex == _path.finishLineIndex)
                    {
                        followingPath = false;
                        break;
                    }
                    else
                    {
                        pathIndex++;
                    }
                }

                if (followingPath)
                {

                    if (pathIndex >= _path.slowDownIndex && _stoppingDst > 0)
                    {
                        speedPercent =
                            Mathf.Clamp01(_path.turnBoundaries[pathIndex].DistanceFromPoint(pos2D) / _stoppingDst);
                        if (speedPercent < 0.01f)
                        {
                            break;
                        }
                    }

                    var direction = _path.lookPoints[pathIndex] - currentPos;
                    var targetRotation = quaternion.LookRotation(direction, new float3(0,1,0));
                    currentRot = math.nlerp(currentRot, targetRotation, deltaTime * turnSpeed);
                    currentPos += math.mul(currentRot, new float3(0, 0, 1)) * (movementSpeed * deltaTime * speedPercent);
                    yield return (currentPos, currentRot);
                }
            }
        }

        # region Debug for unity editor
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!JHUtility.IsInScene(gameObject)) return;
            var map = _map;
            if (map == null)
            {
                foreach (var m in FindObjectsOfType<Map>())
                {
                    if (m.OnMap(transform.position))
                        map = m;
                }
            }
            if (map == null) return;
            var mapPos = map.GetMapPosition(transform.position);
            var worldPos = map.GetWorldPosition(mapPos);

            var currentColor = Gizmos.color;
            var c = Color.yellow;
            c.a = .3f;
            Gizmos.color = c;
            Gizmos.DrawCube(worldPos, map.Distance);
            Gizmos.color = currentColor;
            if (_target != null)
            {
                var targetMapPos = map.GetMapPosition(_target.position);
                var targetWorldPos = map.GetWorldPosition(targetMapPos);
                Gizmos.DrawCube(targetWorldPos, map.Distance);
            }
        }

        private void OnDrawGizmosSelected()
        {
            DrawPath();
        }
        public void EditorCalculatePath()
        {
            var map = _map;
            if (map == null)
            {
                foreach (var m in FindObjectsOfType<Map>())
                {
                    if (m.OnMap(transform.position))
                        map = m;
                }
            }

            if (map == null) return;
            Vector3 startPos = transform.position;
            Vector3 endPos = _target.position;
            map.PathFindingWithAstar(startPos, endPos, out var wayPoint);
            if (wayPoint == null) return;
            
            _path = new Path( wayPoint.ToArray(), transform.position, _turnDst, _stoppingDst);
            wayPoint = null;
        }
        private void DrawPath()
        {
            if (_path == null) return;
            var map = _map;
            if (map == null)
            {
                foreach (var m in FindObjectsOfType<Map>())
                {
                    if (m.OnMap(transform.position))
                        map = m;
                }
            }

            if (map == null ) return;
            if (_path != null)
            {
                _path.DrawWithGizmos();
            }
        }
#endif
        #endregion
    }
}