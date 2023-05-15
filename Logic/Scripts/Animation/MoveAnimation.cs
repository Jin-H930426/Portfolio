using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace JH.Portfolio.Animation
{
    public class MoveAnimation : ScriptAnimation
    {
        private Spline _path;
        public float duration = 1f;
        public float tension = 1f;
        public bool isLocal = false;
        public float3[] path;

        private void Awake()
        {
            _path = new Spline(SplineType.CatmullRom, path, isLoop, 1);
        }

        public override IEnumerator Animation_Coroutine()
        {
            yield return isLoop ? LoopAnimation_Coroutine() : NoneLoopAnimation_Coroutine();
        }

        IEnumerator LoopAnimation_Coroutine()
        {
            while (true)
            {
                yield return NoneLoopAnimation_Coroutine();
            }
        }

        IEnumerator NoneLoopAnimation_Coroutine()
        {
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                if (isLocal) transform.localPosition = _path.GetPoint(t);
                else transform.position = _path.GetPoint(t/duration);
                yield return null;
            }
        }
    }
}