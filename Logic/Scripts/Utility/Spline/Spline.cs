using System;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace JH
{
    public enum SplineType
    {
        Bezier = 1,
        Hermite = 2,
        CatmullRom = 4,
        Cardinal = 8,
    }

    [System.Serializable]
    public class Spline
    {
        public static readonly SplineType CAN_POINT_MOVE = (SplineType.Bezier | SplineType.Hermite);
        public SplineType splineType;
        [HideInInspector] public bool isLoop;
        [field: SerializeField] public float3[] Points { get; private set; }
        [field: SerializeField] public float3[] Velocities { get; set; }
        public Line[] Lines { get; set; }

        public float3 this[int index]
        {
            get => Points[index];
            set => Points[index] = value;
        }
        public float Length => Lines.Sum(x => x.Length);
        public float Count => Points.Length;
        public float tension = 0f;

        public Spline(SplineType splineType, float3[] points, bool isLoop, float tension = 0)
        {
            this.splineType = splineType;
            this.isLoop = isLoop;
            this.Points = points;
            this.tension = tension;

            SetVelocityArray();
            SetLineArray();
        }
        public void SetVelocityArray()
        {
            int pointCount = Points.Length;
            if (pointCount == 0) return;
            var currentVelocities = Velocities;
            Velocities = new float3[pointCount];
            Lines = new Line[isLoop ? pointCount : pointCount - 1];
            
            var cardinalTension = splineType == SplineType.Cardinal ? this.tension : 0;

            for (int i =0; i < pointCount; i++)
            {
                var center = i % Points.Length;
                if ((splineType & CAN_POINT_MOVE) != 0 && i < currentVelocities.Length)
                    Velocities[center] = currentVelocities[center];
                else
                    SetVelocity(center);
            }

            if (!isLoop && (splineType & CAN_POINT_MOVE) == 0)
            {
                Velocities[0] = 0;
                Velocities[pointCount - 1] = 0;
            }

            void SetVelocity(int center)
            {
                var index0 = (center - 1 + pointCount) % pointCount;
                var index1 = center % Points.Length;
                var index2 = (center + 1) % Points.Length;

                var p0 = Points[index0];
                var p1 = Points[index1];
                var p2 = Points[index2];

                Velocities[index1] = GetVelocity(p0, p2, cardinalTension);
            } 
        }
        public void SetLineArray()
        {
            int pointCount = Points.Length;
            if (pointCount == 0) return;
            
            
            for (int i = 0; i < Lines.Length; i++)
            {
                SetLine(i); 
            }
            void SetLine(int index)
            {
                var i0 = index % pointCount;
                var i1 = (index + 1) % pointCount;
                var p0 = Points[i0];
                var p1 = Points[i1];
                var u = Velocities[i0];
                var v = Velocities[i1];
                var bezier = GetBezierPointFromUV(p0, p1, u, v);
                Lines[i0] = new Line()
                {
                    P0 = bezier.p0,
                    P1 = bezier.p1,
                    P2 = bezier.p2,
                    P3 = bezier.p3,
                }; 
            }
        }
        
        public void Update(SplineType splineType, float3[] newPoints, bool isLoop, float tension = 0)
        {
            this.splineType = splineType;
            this.isLoop = isLoop;
            Points = newPoints;
            this.tension = tension;

            SetVelocityArray();
            SetLineArray();
        }
        public float3 GetPoint(float delta)
        {
            var distance = delta * Lines.Length;
            int index = (int)math.floor(distance);
            float weight = distance - index;
            return Lines[index][weight];
        }

        public static float3 GetBezierPoint(float3 p0, float3 p1, float3 p2, float3 p3, float t)
        {
            var tt = t * t;
            var ttt = tt * t;
            var u = 1 - t;
            var uu = u * u;
            var uuu = uu * u;
            // B(t) = uuu*po + 3uu*p1*t + 3u*p2*tt + p3*ttt
            return uuu * p0 + 3 * uu * t * p1 + 3 * u * tt * p2 + p3 * ttt;
        }
        public static float3 GetFirstDerivative(float3 p0, float3 p1, float3 p2, float3 p3, float t)
        {
            var tt = t * t;
            var u = 1 - t;
            var uu = u * u;
            // B'(t) = 3uu*(p1-p0) + 6ut*(p2-p1) + 3tt*(p3-p2)
            return 3 * uu * (p1 - p0) + 6 * u * t * (p2 - p1) + 3 * tt * (p3 - p2);
        }
        /// <summary>
        /// Calculate velocity with cardinal spline
        /// </summary>
        /// <param name="p0">point</param>
        /// <param name="p2"></param>
        /// <param name="tension"></param>
        /// <returns></returns>
        public static float3 GetVelocity(float3 p0, float3 p2, float tension)
        {
            return (1 - tension) * .5f * (p2 - p0);
        }
        
        /// <summary>
        /// p0' = p0,
        /// p1' = p0 + u / 3f,
        /// p2' = p1 - v / 3f,
        /// p3' = p1
        /// </summary>
        /// <param name="p0">point 0</param>
        /// <param name="p1">point 1</param>
        /// <param name="u">point 0's velocity</param>
        /// <param name="v">point 1's velocity</param>
        /// <returns></returns>
        public static (float3 p0, float3 p1, float3 p2, float3 p3) GetBezierPointFromUV(float3 p0, float3 p1,
            float3 u, float3 v)
        {
            return (p0, p0 + u / 3f, p1 - v / 3f, p1);
        }

        /// <summary>
        /// p0 = p0',
        /// p1 = p3',
        /// u = (p1' - p0') * 3f,
        /// v = -(p2' - p3') * 3f = (p3' - p2') * 3f
        /// </summary>
        /// <param name="p0">point 0</param>
        /// <param name="p1">guide point 0</param>
        /// <param name="p2">guide point 1</param>
        /// <param name="p3">tangent 1</param>
        /// <returns></returns>
        public static (float3 p0, float3 p1, float3 u, float3 v) GetUVFromBezierPoint(float3 p0, float3 p1,
            float3 p2, float3 p3)
        {
            return (p0, p3, 3f * (p1 - p0), 3f * (p3 - p2));
        }

#if UNITY_EDITOR 
        public void DrawGizmo()
        {
            if (Lines == null) return;

            foreach (var line in Lines)
            {
                line.DrawGizmo();
            }
        }
#endif
    }

    [System.Serializable]
    public struct Line
    {
        public float3 P0 { get; set; }
        public float3 P1 { get; set; }
        public float3 P2 { get; set; }
        public float3 P3 { get; set; }

        public float DirctionDistance => math.distance(P0, P3);
        public float delta => math.clamp(1f / (DirctionDistance * 10f), .0001f, .1f);
        public float Length
        {
            get
            {
                var length = 0f;
                for (float t = 0; t < 1; t += delta)
                {
                    var p = this[t];
                    var p2 = this[t + delta];
                    length += math.distance(p, p2);
                }

                return length;
            }
        }

        public float3 this[float t] => Spline.GetBezierPoint(P0, P1, P2, P3, t);
        public (float3 p0, float3 p1, float3 u, float3 v) GetUVFromBezierPoint() =>Spline.GetUVFromBezierPoint(P0, P1, P2, P3);

#if UNITY_EDITOR
        public void DrawGizmo()
        {
            Gizmos.color = Color.green;
            // Draw Path Line
            for (float t = 0; t < 1; t += .001f)
            {
                var p = this[t];
                var p2 = this[t + .001f];
                Gizmos.DrawLine(p, p2);
                Gizmos.DrawCube(p, Vector3.one * .01f);
            }
            Gizmos.color = Color.white;
        }
#endif
    }
}