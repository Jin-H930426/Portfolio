using System;
using JH.Portfolio.Manager;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

namespace JH.Portfolio.Map
{
    public class Test : MonoBehaviour
    {
        [System.Serializable]
        public class MouseEvent : UnityEvent<float3> { }

        public MouseEvent onMouseClick;

        public void Start()
        {
            GameManager.InputManager.OnAttackInputEvent.OnPressedEvent += MouseClickEvent;
        }

        public void OnDestroy()
        {
            if (GameManager.InputManager != null)
                GameManager.InputManager.OnAttackInputEvent.OnPressedEvent -= MouseClickEvent;
        }

        public void MouseClickEvent()
        {
            var mousePosition = UnityEngine.Input.mousePosition;
            mousePosition.z = transform.position.y - UnityEngine.Camera.main.transform.position.y;
            var pointToWorld = UnityEngine.Camera.main.ScreenToWorldPoint(mousePosition);
            onMouseClick.Invoke(pointToWorld);
        }
    }
}