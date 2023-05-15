using UnityEngine;
using UnityEngine.Serialization;

namespace JH.Portfolio.InputSystem
{
    [System.Serializable]
    public struct MovementInput
    {
        public KeyCode forward;
        public KeyCode backward;
        public KeyCode left;
        public KeyCode right;
    }
    [System.Serializable]
    public struct AxisInput
    {
        public string axisX;
        public string axisY;
    }
}