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
        public TMP_Text DurationText;
        
        public TMP_Text ImpactText;
        public Slider DurationSlider;
        public Slider PeriodSlider;

        private AbstractGameplayEffectShelfContainer Container;
        private bool initialized;

        public void Set(ref AbstractGameplayEffectShelfContainer container)
        {
            Container = container;
            initialized = true;
            
            EffectNameText.text = Container.Spec.Base.GetIdentifier().Name;
            DurationText.text = Container.DurationRemaining.ToString("F1");
            
            /*float impactMagnitude = Container.Spec.Base.ImpactSpecification.GetMagnitude(Container.Spec);
            float secondRate = 1 / Container.PeriodDuration * impactMagnitude;
            string impactText = $"{secondRate}/s ({impactMagnitude}/t) (={impactMagnitude * (Container.Spec.Base.DurationSpecification.Ticks + (Container.Spec.Base.DurationSpecification.TickOnApplication ? 1 : 0))})";
            
            ImpactText.text = $"{Container.Spec.Base.ImpactSpecification.AttributeTarget.Name}: {impactText}";*/

            ImpactText.text = GetImpactString();
        }

        private string GetImpactString()
        {
            string lastImpactString = "N/A";
            string trackedImpactString = "N/A";
            if (Container.TryGetLastTrackedImpact(out AttributeValue lastImpact)) lastImpactString = lastImpact.ToString();
            if (Container.TryGetTrackedImpact(out AttributeValue trackedImpact)) trackedImpactString = trackedImpact.ToString();

            return $"{lastImpactString} ({trackedImpactString})";
        }

        public void LateUpdate()
        {
            if (!initialized) return;
            if (Container.DurationRemaining <= 0f || !Container.Valid)
            {
                ImpactText.text = GetImpactString();
                initialized = false;
                EffectDebugManager.Instance.BackCommunicate(this);
            }
            else
            {
                ImpactText.text = GetImpactString();
                DurationText.text = Container.DurationRemaining.ToString("F1");
                DurationSlider.value = Container.DurationRemaining / Container.TotalDuration;
                PeriodSlider.value = 1 - Container.TimeUntilPeriodTick / Container.PeriodDuration;
            }
        }
    }
}
