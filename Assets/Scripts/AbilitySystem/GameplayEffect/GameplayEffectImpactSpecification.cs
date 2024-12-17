using System;
using System.Collections.Generic;

namespace FESGameplayAbilitySystem
{
    [Serializable]
    public class GameplayEffectImpactSpecification
    {
        public AttributeScriptableObject AttributeTarget;
        public float Magnitude;
        public List<AbstractGameplayEffectCalculationScriptableObject> MagnitudeCalculation;

        public void ApplyImpactSpecifications(GameplayEffectSpec spec)
        {
            foreach (AbstractGameplayEffectCalculationScriptableObject calculation in MagnitudeCalculation)
            {
                calculation.Initialize(spec);
            }
        }
    }
}
