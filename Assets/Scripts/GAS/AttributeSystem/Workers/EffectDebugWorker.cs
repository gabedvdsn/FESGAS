using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FESGameplayAbilitySystem
{
    public class EffectDebugWorker : MonoBehaviour
    {
        public CanvasGroup Canvas;
        
        public TMP_Text EffectNameText;
        public TMP_Text ImpactText;
        public Slider DurationSlider;
        public Slider PeriodSlider;

        private GameplayEffectShelfContainer Container;
        private bool initialized;

        public void Set(ref GameplayEffectShelfContainer container)
        {
            Container = container;
            initialized = true;
            
            EffectNameText.text = Container.Spec.Base.name;

            float impactMagnitude = Container.Spec.Base.ImpactSpecification.GetMagnitude(Container.Spec);
            float secondRate = 1 / Container.PeriodDuration * impactMagnitude;
            string impactText = $"{secondRate}/s ({impactMagnitude}/t) (={impactMagnitude * (Container.Spec.Base.DurationSpecification.Ticks + (Container.Spec.Base.DurationSpecification.TickOnApplication ? 1 : 0))})";
            
            ImpactText.text = $"{Container.Spec.Base.ImpactSpecification.AttributeTarget.Name}: {impactText}";
        }

        public void Update()
        {
            if (!initialized) return;
            if (Container.DurationRemaining <= 0f || !Container.Valid)
            {
                initialized = false;
                EffectDebugManager.Instance.BackCommunicate(this);
            }
            else
            {
                DurationSlider.value = Container.DurationRemaining / Container.TotalDuration;
                PeriodSlider.value = 1 - Container.TimeUntilPeriodTick / Container.PeriodDuration;
            }
        }
    }
}
