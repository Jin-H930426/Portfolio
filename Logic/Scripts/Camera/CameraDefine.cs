using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Serialization;

namespace JH.Portfolio.Camera
{
    
    [System.Serializable]
    public struct CameraLens
    {
        public enum ProjectionType
        {
            Perspective,
            Orthographic
        } 
        #region Define 
        public static readonly CameraLens Default = new CameraLens
        {
            projectionType = ProjectionType.Perspective,
            fieldOfView = 60f,
            orthographicSize = 5,
            clippingPlane = new Vector2(0.3f, 1000f)
        };
        #endregion
        
        public ProjectionType projectionType;
        public float fieldOfView;
        public float orthographicSize;
        [FormerlySerializedAs("ClippingPlane")] [MinMaxSlider(0.001f, 1000)] public Vector2 clippingPlane;
        
        public static CameraLens Lerp(CameraLens from, CameraLens to, float t)
        {
            return new CameraLens
            {
                projectionType = t < 0.5f ? from.projectionType : to.projectionType,
                orthographicSize = Mathf.Lerp(from.orthographicSize, to.orthographicSize, t),
                fieldOfView = Mathf.Lerp(from.fieldOfView, to.fieldOfView, t),
                clippingPlane = Vector2.Lerp(from.clippingPlane, to.clippingPlane, t)
            };
        }
    }
    
    [System.Serializable]
    public enum CameraMode : int
    {
        TheFirstPersonView = 1,
        ThirdPersonView = 2,
        FreeView = 4
    }
}