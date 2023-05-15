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
        private AstarMapBaker _astarMapBaker;
        [SerializeField] private Spline _spline;
        // [SerializeField, ReadOnly] Path _path;
        
        [ContextMenuItem("Calculate Path", "EditorCalculatePath")]
        [SerializeField] Transform _target;

        [SerializeField] private float _turnDst = 5;
        [SerializeField] private float _stoppingDst = 10;
        
        public Transform target { get => _target; }
        public float turnDst { get => _turnDst; set => _turnDst = value; }
        public float stoppingDst { get => _stoppingDst; set => _stoppingDst = value; }
        
        private void Start()
        {
            _astarMapBaker = AstarMapBaker.FindMapWith(transform.position);
        }

        public void CalculatePath(Vector3 endPos)
        {
            CalculatePath(transform.position, endPos);
        }
        public void CalculatePath(Vector3 startPos, Vector3 endPos)
        {
            _astarMapBaker.PathFindingWithAstar(startPos, endPos, out var wayPoint);
            if (wayPoint == null) return;
            var w = wayPoint.ToArray();
            // _path = new Path(w, transform.position, _turnDst, _stoppingDst);k
            _spline = new Spline(SplineType.CatmullRom, w, false);
            wayPoint = null;
        }
        
        

        # region Debug for unity editor
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!JHUtility.IsInScene(gameObject)) return;
        }

        private void OnDrawGizmosSelected()
        {
            EditorPath();
        }

        public void EditorPath()
        {
            if (_spline == null) return;
            _spline.DrawGizmo();
        }
#endif
        #endregion
    }
}