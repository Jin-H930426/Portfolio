using System;
using UnityEngine;

namespace JH.Portfolio.Animation
{
    public class SupportAnimation : MonoBehaviour
    {
        [ContextMenu("Initialized Animation Dictionary")]
        public void InitAnimationDictionary()
        {
            if (_animator == null) return;
            animationDictionary.Clear();
            foreach (var animation in _animator.runtimeAnimatorController.animationClips)
            {
                if (animationDictionary.ContainsKey(animation.name)) continue;
                animationDictionary.Add(animation.name.ToLower().Substring(1).Replace("_", ""), animation.name);
            }
        }

        public Animator _animator;
        public SerializedDictionary<string, string> animationDictionary = new SerializedDictionary<string, string>();
        
        [Header("Animation Play Settings")]
        [SerializeField] private bool _isCrossFade = false;
        [SerializeField] private int _layer = 0;
        [SerializeField] private float _crossFadeTime = 0;
        [SerializeField] private float _normalizedTime = 0;
        
        private void Awake()
        {
            if (!_animator) _animator = GetComponent<Animator>();
        }
        
        public void SetAnimation(bool isCrossFade, int layer, float crossFadeTime, float normalizedTime, float speed = 1)
        {
            _isCrossFade = isCrossFade;
            _layer = layer;
            _crossFadeTime = crossFadeTime;
            _normalizedTime = normalizedTime;
            _animator.speed = speed;
        }
        public void PlayAnimation(string animationName)
        {
            if (_isCrossFade) PlayAnimation(animationName, _layer, _crossFadeTime, _normalizedTime);
            else PlayAnimation(animationName, _layer);
        }
        void PlayAnimation(string animationName, int layer, float crossFadeTime, float normalizedTime)
        {
            if (!animationDictionary.ContainsKey(animationName)) return;
            _animator.CrossFade(animationDictionary[animationName], crossFadeTime, layer, normalizedTime);
        }
        void PlayAnimation(string animationName, int layer)
        {
            _animator.Play(animationDictionary[animationName], layer);
        }
    }
}