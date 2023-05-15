using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace JH
{
    public class SplineComponent : MonoBehaviour
    {
        private Spline _spline;
        
        [ReadOnlyProperty, SerializeField] public float3 position;
        [Header("Spline Properties")]
        public SplineType splineType;
        public bool isLoop = false;
        public float tension = 0f;
        public List<float3> points = new List<float3>();
        
        [Header("Result"), ReadOnlyProperty] public float splineLenght;
        
        [HideInInspector] public bool isLocalSpace = false;
        public bool IsLocalSpace
        {
            get => isLocalSpace;
            set
            {
                if (isLocalSpace == value) return;
                if (value == true)
                    points = points.Select(x=> x - position).ToList();
                else points = points.Select(x=> x + position).ToList();
                isLocalSpace = value;
                SetSpline();
            }
        }
        
        public void SetSpline()
        {
            if(_spline == null)
            {
                _spline = new Spline(splineType, IsLocalSpace ? 
                    points.Select(x=>position + x).ToArray() : 
                    points.ToArray(), isLoop, tension);
                return;
            }
            
            _spline.Update(splineType, IsLocalSpace ? 
                points.Select(x=>position + x).ToArray() : 
                points.ToArray(), isLoop, tension);
            
            splineLenght = _spline.Length;
        }

#if UNITY_EDITOR
        [Header("Editor Properties")]
        [SerializeField] bool checkLocalSpace = false;

        public Spline EditorSpline => _spline;
        private void OnDrawGizmos()
        {
            float3 newPosition = transform.position;
            if (!position.Equals(newPosition))
            {
                position = newPosition;
                SetSpline();
            }
            if (_spline == null) return;

            _spline.DrawGizmo();
        }
        private void OnValidate()
        {
            if(checkLocalSpace != isLocalSpace)
                IsLocalSpace = checkLocalSpace;
            SetSpline();
        }
#endif
    }
}