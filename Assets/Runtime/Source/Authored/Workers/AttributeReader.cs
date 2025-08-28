using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FESGameplayAbilitySystem
{
    public class AttributeReader : MonoBehaviour, IAttributeAssignable
    {
        public TMP_Text CurrentText;
        public TMP_Text BaseText;
        public Slider ValueSlider;

        private AttributeSystemComponent source;
        private Attribute attribute;
        
        private void LateUpdate()
        {
            if (!source) return;
            if (!source.TryGetAttributeValue(attribute, out AttributeValue attributeValue)) return;

            CurrentText.text = attributeValue.CurrentValue.ToString(CultureInfo.InvariantCulture);
            BaseText.text = attributeValue.BaseValue.ToString("F2");

            float targetValue = attributeValue.CurrentValue / attributeValue.BaseValue;
            ValueSlider.value = Mathf.Lerp(ValueSlider.value, targetValue, Time.deltaTime * 10f);
        }
        public void AssignAttribute(Attribute attr)
        {
            attribute = attr;
        }
        public void AssignSystem(AttributeSystemComponent asc)
        {
            source = asc;
        }
    }

    public interface IAttributeAssignable
    {
        public void AssignAttribute(Attribute attr);
        public void AssignSystem(AttributeSystemComponent asc);
    }
}
