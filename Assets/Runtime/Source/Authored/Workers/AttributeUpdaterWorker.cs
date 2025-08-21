using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FESGameplayAbilitySystem
{
    public class AttributeUpdaterWorker : MonoBehaviour, IAttributeAssignable
    {
        public AttributeSystemComponent Source;
        public AttributeScriptableObject AttributeTarget;

        public TMP_Text CurrentText;
        public TMP_Text BaseText;
        public Slider ValueSlider;

        private IAttribute attribute;

        private void Awake()
        {
            attribute = AttributeTarget;
        }

        private void LateUpdate()
        {
            if (!Source.TryGetAttributeValue(attribute, out AttributeValue attributeValue)) return;

            CurrentText.text = attributeValue.CurrentValue.ToString(CultureInfo.InvariantCulture);
            BaseText.text = attributeValue.BaseValue.ToString("F2");

            float targetValue = attributeValue.CurrentValue / attributeValue.BaseValue;
            ValueSlider.value = Mathf.Lerp(ValueSlider.value, targetValue, Time.deltaTime * 10f);
        }
        public void AssignAttribute(IAttribute attr)
        {
            attribute = attr;
        }
    }

    public interface IAttributeAssignable
    {
        public void AssignAttribute(IAttribute attr);
    }
}
