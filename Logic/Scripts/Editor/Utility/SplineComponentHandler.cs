using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace JH
{
    [CustomEditor(typeof(SplineComponent))]
    public class SplineComponentHandler : Editor
    {
        int _pointIndex = -1;
        int _guidePointCanMove = -1;
        Camera SceneCamera => SceneView.lastActiveSceneView.camera;
        private void OnSceneGUI()
        {
            if (target is not SplineComponent splineComponent) return;
            if (splineComponent.EditorSpline is not Spline spline) return;
            
            EditorGUI.BeginChangeCheck();
            if(_pointIndex >= splineComponent.points.Count) _pointIndex = -1;
            
            var tool = Tools.current;
            Tools.current = Tool.None;
            HandlePointsEnter(splineComponent);
            HandlePointMove(splineComponent, spline);
            HandleHermiteVector(spline);
            HandleBezierGuidePoint(spline);
            Tools.current = tool;
            if (EditorGUI.EndChangeCheck())
            {
                splineComponent.SetSpline();

                Undo.RecordObject(splineComponent, "MoveTangent");
                EditorUtility.SetDirty(splineComponent);
            }
        }

        void HandlePointsEnter(SplineComponent splineComponent)
        {
            for (var i = 0; i < splineComponent.points.Count; i++)
            {
                var point = splineComponent.IsLocalSpace
                    ? (float3)splineComponent.transform.TransformPoint(splineComponent.points[i])
                    : splineComponent.points[i];
                
                var dis = Vector3.Distance(SceneCamera.transform.position, point);
                if (Handles.Button(point, Quaternion.identity,
                        0.3f, 0.3f, Handles.SphereHandleCap))
                {
                    _pointIndex = i;
                    _guidePointCanMove = -1;
                }
            }
        }

        void HandlePointMove(SplineComponent splineComponent, Spline spline)
        {
            if (_pointIndex == -1) return;

            var point = splineComponent.IsLocalSpace
                ? (float3)splineComponent.transform.TransformPoint(splineComponent.points[_pointIndex])
                : splineComponent.points[_pointIndex];

            point = Handles.FreeMoveHandle(point, Quaternion.identity, .3f, 
                Vector3.zero, Handles.SphereHandleCap);
            splineComponent.points[_pointIndex] = splineComponent.IsLocalSpace // check local space type
                ? splineComponent.transform
                    .InverseTransformPoint(point) // if local space, transform point to local space
                : point; // else just set point
        }

        void HandleBezierGuidePoint(Spline spline)
        {
            if (_pointIndex == -1 || spline.splineType != SplineType.Bezier) return;

            float3 uPoint = spline.Lines[_pointIndex].P1;
            float3 vPoint = spline.Lines[_pointIndex].P2;

            Handles.color = Color.gray;

            uPoint = Handles.FreeMoveHandle(uPoint, Quaternion.identity, 0.5f,
                Vector3.zero, Handles.SphereHandleCap);
            vPoint = Handles.FreeMoveHandle(vPoint, Quaternion.identity, 0.5f,
                Vector3.zero, Handles.SphereHandleCap);

            SetLine();

            void SetLine()
            {
                var line = spline.Lines[_pointIndex];
                line.P1 = uPoint;
                line.P2 = vPoint;

                var uv = line.GetUVFromBezierPoint();
                Handles.color = Color.gray;
                Handles.DrawLine(line.P0, line.P1, 1);
                Handles.DrawLine(line.P2, line.P3, 1);
                Handles.DrawBezier(line.P0, line.P3, line.P1, line.P2, Color.white, null, 4f);
                spline.Velocities[_pointIndex] = uv.u;
                if (spline.isLoop)
                    spline.Velocities[(_pointIndex + 1) % spline.Velocities.Length] = uv.v;
                else if (spline.Velocities.Length > _pointIndex + 1)
                    spline.Velocities[_pointIndex + 1] = uv.v;
            }
        }

        void HandleHermiteVector(Spline spline)
        {
            if (_pointIndex == -1 || spline.splineType == SplineType.Bezier) return;

            float3 velocity = spline.Velocities[_pointIndex];

            float3 start = spline.Points[_pointIndex];
            float3 end = start + velocity;
            Quaternion direction = Quaternion.LookRotation(velocity.Equals(float3.zero)
                ? Vector3.forward
                : velocity);
            Handles.color = Color.red;
            Handles.DrawLine(start, end, 2);
            Handles.ConeHandleCap(0, end, direction, .3f, EventType.Repaint);

            if (spline.splineType == SplineType.Hermite)
            {
                Handles.color = Color.clear;
                spline.Velocities[_pointIndex]
                    = (float3)Handles.FreeMoveHandle(end, direction, .3f,
                        Vector3.zero, Handles.ConeHandleCap) - start;
            }
        }
    }
}