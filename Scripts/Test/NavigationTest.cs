using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JH.Portfolio.Test
{
    using Map;
    public class NavigationTest : MonoBehaviour
    {
        MapObject _mapObject;
        public Vector3 previousTargetPosition;
        public float newTargetMargin = 0.1f;
        public float moveSpeed = 5f;
        public float turnSpeed = 30f;
        
        
        void Start()
        {
            _mapObject = GetComponent<MapObject>();
            StopCoroutine("PathFinding");
            StartCoroutine("PathFinding");
        }
        
        IEnumerator PathFinding()
        {
            while (true)
            {
                while (!_mapObject.target)
                {
                    yield return null;
                }

                yield return new WaitForSeconds(.2f);
                var margin = previousTargetPosition - _mapObject.target.position;
                if (margin.sqrMagnitude > newTargetMargin * newTargetMargin)
                {
                    var position = _mapObject.target.position;
                    
                    _mapObject.CalculatePath(position);
                    previousTargetPosition = position;
                    
                    StopCoroutine("MovePath");
                    StartCoroutine("MovePath");
                }
            }
        }

        IEnumerator MovePath()
        {
            float deltaTime = Time.fixedDeltaTime;
            var movement = _mapObject.CalculationMovement(transform.position, transform.rotation,
                Time.fixedDeltaTime, moveSpeed, turnSpeed);
            while (movement.MoveNext())
            {
                var (pos, rot) = movement.Current;
                transform.SetPositionAndRotation(pos, rot);
                yield return new WaitForFixedUpdate();
            }
        }
    }
}