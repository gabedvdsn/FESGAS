using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FESGameplayAbilitySystem
{
    public class AttributeUpdaterWorker : MonoBehaviour
    {
        public AttributeSystemComponent Source;
        public AttributeScriptableObject AttributeTarget;

        public TMP_Text CurrentText;
        public TMP_Text BaseText;
        public Slider ValueSlider;
        
        private void LateUpdate()
        {
            if (!Source.TryGetAttributeValue(AttributeTarget, out CachedAttributeValue attributeValue)) return;

            CurrentText.text = attributeValue.Value.CurrentValue.ToString(CultureInfo.InvariantCulture);
            BaseText.text = attributeValue.Value.BaseValue.ToString(CultureInfo.InvariantCulture);

            float targetValue = attributeValue.Value.CurrentValue / attributeValue.Value.BaseValue;
            ValueSlider.value = Mathf.Lerp(ValueSlider.value, targetValue, Time.deltaTime * 10f);
        }
    }
}
