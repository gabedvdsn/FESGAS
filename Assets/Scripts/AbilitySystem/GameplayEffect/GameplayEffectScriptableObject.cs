using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(menuName = "FESGAS/Gameplay Effect", fileName = "New Gameplay Effect")]
    public class GameplayEffectScriptableObject : ScriptableObject
    {
        public GameplayTagScriptableObject Identifier;
        
        public GameplayEffectPolicySpecification PolicySpecification;
        public GameplayEffectImpactSpecification ImpactSpecification;
        
        public GameplayEffectRequirements Requirements;

        public GameplayEffectSpec Generate(AbilitySystemComponent target, AbilitySystemComponent source)
        {
            GameplayEffectSpec spec = new GameplayEffectSpec
            {
                Base = this,
                Target = target,
                Source = source
            };

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
    }
}
