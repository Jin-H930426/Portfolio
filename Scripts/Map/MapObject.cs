using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace JH.Portfolio.Map
{
    public class MapObject : MonoBehaviour
    {
        private Map _map;

        private Vector2Int _StartPos;
        private void Start()
        {
            _map = Map.GetOnMap(transform.position);
        }
        
        #if UNITY_EDITOR
        private void OnDrawGizmos()
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
            var (x, y) = map.GetMapPosition(transform.position);
            var worldPos = map.GetWorldPosition(x, y);

            var currentColor = Gizmos.color;
            var c = Color.yellow;
            c.a = .3f;
            Gizmos.color = c;
            Gizmos.DrawCube(worldPos, map.Distance);
            Gizmos.color = currentColor;
        }
        #endif
    }
}