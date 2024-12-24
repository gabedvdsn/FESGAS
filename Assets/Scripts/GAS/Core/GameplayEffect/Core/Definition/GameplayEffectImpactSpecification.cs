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
        public EffectImpactTarget ValueTarget;
        public CalculationOperation ImpactOperation;
        public bool ReverseImpactOnRemoval;
        
        [Space]
        
        public float Magnitude;
        public AbstractMagnitudeModifierScriptableObject MagnitudeCalculation;
        [FormerlySerializedAs("MagnitudeOperation")] public MagnitudeOperation MagnitudeCalculationOperation;

        public void ApplyImpactSpecifications(GameplayEffectSpec spec)
        {
            MagnitudeCalculation.Initialize(spec);
        }

        public float GetMagnitude(GameplayEffectSpec spec)
        {
            float calculatedMagnitude = MagnitudeCalculation.Evaluate(spec);
            
            return MagnitudeCalculationOperation switch
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

    public enum EffectImpactTarget
    {
        Current,
        Base,
        CurrentAndBase
    }
}
