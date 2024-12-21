﻿using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace FESGameplayAbilitySystem
{
    public class SliderManager : MonoBehaviour
    {
        public static SliderManager Instance;
        public CanvasGroup Canvas;

        private void Awake()
        {
            Instance = this;
            
            ToggleSlider(false);
        }

        public Slider Slider;

        public void SetValue(float value) => Slider.value = value;

        public void ToggleSlider(bool flag)
        {
            if (!flag) Slider.value = 0f;
            StartCoroutine(DoFade(flag ? 1f : 0f, .75f));
        }

        private IEnumerator DoFade(float targetAlpha, float duration)
        {
            float initialAlpha = Canvas.alpha;
            float elapsedDuration = 0f;
            while (elapsedDuration < duration)
            {
                Canvas.alpha = Mathf.Lerp(initialAlpha, targetAlpha, elapsedDuration / duration);
                elapsedDuration += Time.deltaTime;
                yield return null;
            }
        }
    }
}
