using System;
using System.Collections.Generic;
using System.Linq;
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
        public EGameplayEffectApplicationPolicy ReApplicationPolicy;
        
        [Space]
        
        public float Magnitude;
        public AbstractMagnitudeModifierScriptableObject MagnitudeCalculation;
        public EMagnitudeOperation MagnitudeCalculationOperation;

        [Space] [Header("Contained Effects")] 
        
        public List<ContainedEffectPacket> Packets;
        

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

        public List<GameplayEffectScriptableObject> GetContainedEffects(EApplyDuringRemove policy)
        {
            return Packets.Where(packet => packet.Policy == policy).Select(p => p.ContainedEffect).ToList();
        }

        [Serializable]
        public struct ContainedEffectPacket
        {
            public EApplyDuringRemove Policy;
            public GameplayEffectScriptableObject ContainedEffect;
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

    public enum EEffectImpactTargetExpanded
    {
        Current,
        Base,
        CurrentAndBase,
        CurrentOrBase
    }

    public enum EApplyDuringRemove
    {
        OnApply,
        OnTick,
        OnRemove
    }
}
