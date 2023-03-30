using UnityEngine;

namespace JH.Portfolio.Camera
{
    [System.Serializable]
    public struct CameraLens
    {
        #region Define 
        public static readonly CameraLens Default = new CameraLens
        {
            fieldOfView = 60f,
            ClippingPlane = new Vector2(0.3f, 1000f)
        };
        #endregion
        
        
        public float fieldOfView;
        [MinMaxSlider(0.001f, 1000)] public Vector2 ClippingPlane;
        
    }
    
    [System.Serializable]
    public enum CameraMode : int
    {
        TheFirstPersonView = 1,
        ThirdPersonView = 2,
        FreeView = 4
    }
}