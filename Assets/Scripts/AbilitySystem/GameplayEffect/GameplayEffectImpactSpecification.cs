using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace FESGameplayAbilitySystem
{
    [Serializable]
    public class GameplayEffectImpactSpecification
    {
        public AttributeScriptableObject AttributeTarget;
        public CalculationOperation ImpactOperation;
        
        [Space]
        
        public float Magnitude;
        public AbstractGameplayEffectCalculationScriptableObject MagnitudeCalculation;
        public EffectImpactMagnitudeCalculation MagnitudeOperation;

        public void ApplyImpactSpecifications(GameplayEffectSpec spec)
        {
            MagnitudeCalculation.Initialize(spec);
        }

        public float GetMagnitude(GameplayEffectSpec spec)
        {
            float calculatedMagnitude = MagnitudeCalculation.Evaluate(spec);
            
            return MagnitudeOperation switch
            {
                EffectImpactMagnitudeCalculation.Add => Magnitude + calculatedMagnitude,
                EffectImpactMagnitudeCalculation.Multiply => Magnitude * calculatedMagnitude,
                EffectImpactMagnitudeCalculation.OverrideCalculation => calculatedMagnitude,
                EffectImpactMagnitudeCalculation.OverrideMagnitude => Magnitude,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    public enum EffectImpactMagnitudeCalculation
    {
        Add,
        Multiply,
        OverrideCalculation,
        OverrideMagnitude
    }
}
