using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

namespace JH
{
    public class TransformLogic : MonoBehaviour         
    {
        public async Task ToDoMove(Transform target, float3 from, float3 to, float duration)
        {
            float time = 0;
            while (time < duration)
            {
                time += Time.deltaTime;
                target.position = Vector3.Lerp(from, to, time / duration);
                await Task.Yield();
            }
        }
    }
}