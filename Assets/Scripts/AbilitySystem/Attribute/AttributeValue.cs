using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public struct AttributeValue
    {
        public float CurrentValue;
        public float BaseValue;

        public AttributeValue(float currentValue, float baseValue)
        {
            CurrentValue = currentValue;
            BaseValue = baseValue;
        }

        public AttributeValue ApplyModified(ModifiedAttributeValue modifiedAttributeValue)
        {
            return new AttributeValue(
                CurrentValue + modifiedAttributeValue.DeltaCurrentValue,
                BaseValue + modifiedAttributeValue.DeltaBaseValue
            );
        }

        public AttributeValue GetDifference(AttributeValue other)
        {
            return new AttributeValue(
                other.CurrentValue - CurrentValue,
                other.BaseValue - BaseValue
            );
        }
    }

    public struct ModifiedAttributeValue
    {
        public GameplayEffectSpec Spec;
        public float DeltaCurrentValue;
        public float DeltaBaseValue;

        public ModifiedAttributeValue(GameplayEffectSpec spec, float deltaCurrentValue, float deltaBaseValue)
        {
            Spec = spec;
            DeltaCurrentValue = deltaCurrentValue;
            DeltaBaseValue = deltaBaseValue;
        }

        public ModifiedAttributeValue Combine(ModifiedAttributeValue other)
        {
            return new ModifiedAttributeValue(
                Spec, 
                DeltaCurrentValue + other.DeltaCurrentValue, 
                DeltaBaseValue + other.DeltaBaseValue);
        }
    }
}
