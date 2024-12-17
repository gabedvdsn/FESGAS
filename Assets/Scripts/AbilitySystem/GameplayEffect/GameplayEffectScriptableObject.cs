using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(menuName = "FESGAS/Gameplay Effect", fileName = "New Gameplay Effect")]
    public class GameplayEffectScriptableObject : ScriptableObject
    {
        [Header("Gameplay Effect")]
        
        public GameplayTagScriptableObject Identifier;
        public GameplayTagScriptableObject[] GrantedTags;
        
        [Header("Specifications")]
        
        public GameplayEffectPolicySpecification PolicySpecification;
        public GameplayEffectImpactSpecification ImpactSpecification;
        
        [Header("Requirements")]
        
        public GameplayEffectRequirements SourceRequirements;
        public GameplayEffectRequirements TargetRequirements;

        public GameplayEffectSpec Generate(AbilitySystemComponent Source, AbilitySystemComponent Target, float Level)
        {
            GameplayEffectSpec spec = new GameplayEffectSpec(this, Source, Target, Level);

            PolicySpecification.ApplyPolicySpecifications(spec);
            ImpactSpecification.ApplyImpactSpecifications(spec);

            return spec;
        }

    }

    public class GameplayEffectSpec
    {
        public GameplayEffectScriptableObject Base;
        public float Level;

        public float TotalDuration;
        public float DurationRemaining;

        public float PeriodDuration;
        public float TimeUntilPeriodTick;
    
        public AbilitySystemComponent Source;
        public AbilitySystemComponent Target;

        public Dictionary<AbstractGameplayEffectCalculationScriptableObject, AttributeValue?> SourceCapturedAttributes =
            new Dictionary<AbstractGameplayEffectCalculationScriptableObject, AttributeValue?>();

        public GameplayEffectSpec(GameplayEffectScriptableObject gameplayEffect, AbilitySystemComponent source, AbilitySystemComponent target, float level)
        {
            Base = gameplayEffect;
            Source = source;
            Target = target;
            Level = level;
        }
    }

    public class GameplayEffectShelfContainer
    {
        public GameplayEffectSpec Spec;
        public bool Ongoing;

        public GameplayEffectShelfContainer(GameplayEffectSpec spec, bool ongoing)
        {
            Spec = spec;
            Ongoing = ongoing;
        }
    }
}
