using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JH.Portfolio.Animation
{
    public class ScriptAnimation : MonoBehaviour, IAnimationPlay
    {
        public bool activeOnAwake = false;
        public bool isLoop = false;
        
        private void OnEnable()
        {
            if (activeOnAwake) StartCoroutine(Animation_Coroutine());
        }
        public virtual Coroutine Play()
        {
            return StartCoroutine(Animation_Coroutine());
        }


        public virtual IEnumerator Animation_Coroutine()
        {
            yield break;
        }
    }

    public interface IAnimationPlay
    {
        public IEnumerator Animation_Coroutine(); 
    }
}