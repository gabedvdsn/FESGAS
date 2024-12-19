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
        public EffectImpactTargetCalculation ValueTarget;
        public CalculationOperation ImpactOperation;
        
        [Space]
        
        public float Magnitude;
        public AbstractGameplayEffectCalculationScriptableObject MagnitudeCalculation;
        public MagnitudeOperation MagnitudeOperation;

        public void ApplyImpactSpecifications(GameplayEffectSpec spec)
        {
            MagnitudeCalculation.Initialize(spec);
        }

        public float GetMagnitude(GameplayEffectSpec spec)
        {
            float calculatedMagnitude = MagnitudeCalculation.Evaluate(spec);
            
            return MagnitudeOperation switch
            {
                MagnitudeOperation.Add => Magnitude + calculatedMagnitude,
                MagnitudeOperation.Multiply => Magnitude * calculatedMagnitude,
                MagnitudeOperation.UseMagnitude => Magnitude,
                MagnitudeOperation.UseCalculation => calculatedMagnitude,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    public enum MagnitudeOperation
    {
        Multiply,
        Add,
        UseMagnitude,
        UseCalculation
    }

    public enum EffectImpactTargetCalculation
    {
        Current,
        Base,
        CurrentAndBase
    }
}
