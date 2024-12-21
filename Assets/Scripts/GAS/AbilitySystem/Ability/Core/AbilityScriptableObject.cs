using System;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(menuName = "FESGAS/Ability/Ability", fileName = "New Ability")]
    public class AbilityScriptableObject : ScriptableObject
    {
        [Header("Ability")]
        
        public AbilityDefinition Definition;
        public AbilityTags Tags;
        public AbilityProxySpecification Proxy;
        
        [Header("Leveling")]

        public int StartingLevel = 1;
        public int MaxLevel = 4;

        [Header("Using")]
        
        public GameplayEffectScriptableObject Cost;
        public GameplayEffectScriptableObject Cooldown;

        public AbilitySpec Generate(GASComponent Owner, int Level)
        {
            return new AbilitySpec(Owner, this, Level);
        }
    }
    
    public class AbilitySpec
    {
        public GASComponent Owner;
        public AbilityScriptableObject Base;
        public int Level;
        
        public AbilitySpec(GASComponent owner, AbilityScriptableObject ability, int level)
        {
            Owner = owner;
            Base = ability;
            Level = level;
                
            // Proxy = Base.Proxy.
        }

        public void ApplyUsageEffects()
        {
            if (!Base.Cooldown || !(Base.Cooldown.GrantedTags.Length > 0)) return;
            if (!Base.Cost || !Base.Cost.ImpactSpecification.AttributeTarget) return;

            // Apply cost and cooldown effects
            Owner.ApplyGameplayEffect(Owner.GenerateEffectSpec(Owner, Base.Cooldown, Level));
            Owner.ApplyGameplayEffect(Owner.GenerateEffectSpec(Owner, Base.Cost, Level));
        }
            
        public bool ValidateActivationRequirements()
        {
            return !(GetCooldown().DurationRemaining > 0f)
                   && CanCoverCost()
                   && Base.Tags.ValidateSourceRequirements(Owner);
        }

        public bool ValidateActivationRequirements(GASComponent target)
        {
            return ValidateActivationRequirements()
                   && Base.Tags.ValidateTargetRequirements(target);
        }

        public GameplayEffectDuration GetCooldown()
        {
            if (!Base.Cooldown || !(Base.Cooldown.GrantedTags.Length > 0)) return default;
            return Owner.GetLongestDurationFor(Base.Cooldown.GrantedTags);
        }

        public bool CanCoverCost()
        {
            if (!Base.Cost || !Base.Cost.ImpactSpecification.AttributeTarget) return true;
            if (!Owner.AttributeSystem.TryGetAttributeValue(Base.Cost.ImpactSpecification.AttributeTarget, out AttributeValue attributeValue)) return false;
            return attributeValue.CurrentValue >= Base.Cost.ImpactSpecification.GetMagnitude(Owner.GenerateEffectSpec(Owner, Base.Cost, Level));
        }
    }
}
