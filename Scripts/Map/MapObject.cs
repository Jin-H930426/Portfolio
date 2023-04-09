using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace JH.Portfolio.Map
{
    public class MapObject : MonoBehaviour
    {
        private Map _map;
        
        [ContextMenuItem("Calculate Path", "EditorCalculatePath")]
        [SerializeField] Transform _target;
        
        public List<int2> _path;
        private void Start()
        {
            _map = Map.GetOnMap(transform.position);
        }
        
        public void CalculatePath()
        {
            _map.PathFindingWithAstar(_map.GetMapPosition(transform.position), _map.GetMapPosition(_target.position), out _path);
        }
        
        
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
            map.PathFindingWithAstar(map.GetMapPosition(transform.position), map.GetMapPosition(_target.position), out _path);
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

            if (map == null) return;
            var currentColor = Gizmos.color;
            var c = Color.red;
            c.a = .3f;
            Gizmos.color = c;
            foreach (var pos in _path)
            {
                var worldPos = map.GetWorldPosition(pos);
                Gizmos.DrawCube(worldPos, map.Distance);
            }
            Gizmos.color = currentColor;
        }
        #endif
    }
}