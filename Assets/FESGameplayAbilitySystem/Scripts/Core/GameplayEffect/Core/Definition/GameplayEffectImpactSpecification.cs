using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace FESGameplayAbilitySystem
{
    [Serializable]
    public class GameplayEffectImpactSpecification
    {
        [Header("Attribute Impact")]
        
        public AttributeScriptableObject AttributeTarget;
        public EEffectImpactTarget ValueTarget;
        public ECalculationOperation ImpactOperation;

        [Space] 
        
        public EImpactType ImpactType;
        public bool ReverseImpactOnRemoval;
        public GameplayEffectApplicationPolicy ReApplicationPolicy;
        
        [Space]
        
        public float Magnitude;
        public AbstractMagnitudeModifierScriptableObject MagnitudeCalculation;
        public EMagnitudeOperation MagnitudeCalculationOperation;
        
        [Space]
        
        [Header("Contained Effect")]
        public GameplayEffectScriptableObject ContainedEffect;
        

        public void ApplyImpactSpecifications(GameplayEffectSpec spec)
        {
            MagnitudeCalculation.Initialize(spec);
        }

        public float GetMagnitude(GameplayEffectSpec spec)
        {
            float calculatedMagnitude = MagnitudeCalculation.Evaluate(spec);
            
            return MagnitudeCalculationOperation switch
            {
                EMagnitudeOperation.Add => Magnitude + calculatedMagnitude,
                EMagnitudeOperation.Multiply => Magnitude * calculatedMagnitude,
                EMagnitudeOperation.UseMagnitude => Magnitude,
                EMagnitudeOperation.UseCalculation => calculatedMagnitude,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    public enum EMagnitudeOperation
    {
        Multiply,
        Add,
        UseMagnitude,
        UseCalculation
    }

    public enum EEffectImpactTarget
    {
        Current,
        Base,
        CurrentAndBase
    }
}
