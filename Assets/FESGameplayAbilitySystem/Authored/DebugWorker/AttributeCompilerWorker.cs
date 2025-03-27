using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public class AttributeCompilerWorker : MonoBehaviour
    {
        public AttributeSystemComponent Source;
        public AttributeScriptableObject AttributeTarget;
        
        private AttributeValue previousValue;
        private AttributeValue delta;
        
        private float trackingDuration = 1f;
        private Queue<DeltaValue> Changes = new Queue<DeltaValue>();
        private AttributeValue deltaRate;

        private void Awake()
        {
            previousValue = default;
        }

        private void Update()
        {
            // if (!Source.TryGetModifiedAttributeValue(AttributeTarget, out ModifiedAttributeValue attributeValue)) return;
            // Compile(attributeValue);
        }

        private void Compile(AttributeValue attributeValue)
        {
            delta = attributeValue - previousValue;
            if (delta.CurrentValue > Mathf.Epsilon)
            {
                float currentTime = Time.time;
                Changes.Enqueue(new DeltaValue{Time = currentTime, Delta = delta});
                previousValue = attributeValue;
            }

            float cutoffTime = Time.time - trackingDuration;
            while (Changes.Count > 00 && Changes.Peek().Time < cutoffTime) Changes.Dequeue();

            deltaRate = new AttributeValue();
            foreach (DeltaValue value in Changes) deltaRate += value.Delta;
            deltaRate /= trackingDuration;
            
            Output();
        }

        private void Output()
        {
            string styleText = $"{AttributeTarget.Name}/S";
            // OutputText.text = $"{deltaRate.ToString()} {styleText}";
        }
    }

    public struct DeltaValue
    {
        public float Time;
        public AttributeValue Delta;
    }
}
