using System;
using System.Collections;
using System.Collections.Generic;
using JH.Portfolio.Battle;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Debug = JH.Portfolio.Debug;

namespace JH.Portfolio.Battle
{
   public class BattleUI : MonoBehaviour
   {
      public RectTransform background;
      public Text battleStateText;
      private float _height = 0;
      public float t = 0;

      public RectTransform[] charecterCards;
      public RectTransform[] commandCards;
      public RectTransform skillAndItemArea;
      public SkillAndItemCard[] skillAndItemCards;

      public RectTransform[] SelectionPickers;
      public GameObject Popup;
      public Text PopupText;

      public float height
      {
         get => _height;
         set
         {
            _height = Mathf.Clamp01(value);
            background.anchoredPosition = Vector3.Lerp(Vector3.zero, Vector3.up * 400, value);
         }
      }
      
      public void SetCharecterCard(int index, string text)
      {
         charecterCards[index].GetComponentInChildren<Text>().text = text;
      }
      public void SetSkillAndItemCards(int index, Sprite icon, string skillName, string description)
      {
         skillAndItemCards[index].SetCard(icon, skillName, description);
         skillAndItemCards[index].gameObject.SetActive(true);
      }
      
      #region Picker Set
      public void PickTarget(int pickerIndex, RectTransform target)
      {
         var picker = SelectionPickers[pickerIndex];
         picker.anchorMin = target.anchorMin;
         picker.anchorMax = target.anchorMax;
         picker.pivot = target.pivot;
         picker.sizeDelta = target.sizeDelta;
         picker.position = target.position;
         picker.gameObject.SetActive(true);
      }
      public void RelaseTarget(int pickerIndex)
      {
         SelectionPickers[pickerIndex].gameObject.SetActive(false);
      }
      #endregion
      #region Animation 
      public IEnumerator SkillAndItemAreaMove(int direction)
      {
         skillAndItemArea.gameObject.SetActive(true);
         for (; t is >= 0 and <= 1; t += Time.unscaledDeltaTime * direction)
         {
            skillAndItemArea.anchoredPosition = Vector2.Lerp(Vector2.zero, Vector2.up * 800f, t);
            yield return null;
         }
         skillAndItemArea.anchoredPosition = Vector2.Lerp(Vector2.zero, Vector2.up * 800f, direction < 0 ? 0 : 1);
         t = Mathf.Clamp01(t);
         if (direction < 0) skillAndItemArea.gameObject.SetActive(false);
      }
      public IEnumerator ActivePopup(float duration, string text)
      {
         Popup.SetActive(true);
         PopupText.text = text;
         yield return new WaitForSeconds(duration);
         Popup.SetActive(false);
      }
      #endregion
   }
}