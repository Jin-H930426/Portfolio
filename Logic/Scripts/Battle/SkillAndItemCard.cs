using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace JH.Portfolio.Battle
{
    public class SkillAndItemCard : MonoBehaviour
    {
        private RectTransform _rectTransform;
        public Image skillIcon;
        public Text skillName;
        public Text skillDescription;

        public RectTransform rectTransform => _rectTransform;

        private void Awake()
        {
            _rectTransform = (RectTransform)transform;
        }
        public void SetCard(Sprite texture, string skillName, string skillDescription)
        {
            skillIcon.sprite = texture;
            this.skillName.text = skillName;
            this.skillDescription.text = skillDescription;
        }
    }
}