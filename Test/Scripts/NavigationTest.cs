using System.Collections;
using UnityEngine;

namespace JH.Test
{
    using Portfolio.Map;
    public class NavigationTest : MonoBehaviour
    {
        MapObject _mapObject;
        public Vector3 previousTargetPosition;
        public float newTargetMargin = 0.1f;
        public float moveSpeed = 5f;
        public float turnSpeed = 30f;
        public bool isMoving = false;
        public bool onFindPath = false;

        [ContextMenu("SetRandomPosition")]
        public void SetRandomPosition()
        {
            var x = Random.Range(-100, 100);
            var z = Random.Range(-100, 100);

            transform.position = new Vector3(x, 0, z);
        }
        
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
                    onFindPath = true;
                    yield return new WaitWhile(()=>isMoving);
                    yield return null;
                    onFindPath = false;
                }
            }
        }

    }
}