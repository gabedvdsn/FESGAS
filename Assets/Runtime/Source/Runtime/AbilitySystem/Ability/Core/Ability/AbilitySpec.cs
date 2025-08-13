using System;
using System.Collections.Generic;
using System.Linq;

namespace FESGameplayAbilitySystem
{
    public class AbilitySpec : IEffectDerivation
    {
        public ISource Owner;
        public IAbilityData Base;
        public int Level;
        public float RelativeLevel => (Level - 1) / (float)(Base.GetMaxLevel() - 1);
        
        private AbilitySpec(ISource owner, IAbilityData ability, int level)
        {
            Owner = owner;
            Base = ability;
            Level = level;
        }

        public static AbilitySpec Generate(IAbilityData ability, ISource owner, int level = 1)
        {
            return new AbilitySpec(owner, ability, level);
        }

        public void ApplyUsageEffects()
        {
            // Apply cost and cooldown effects
            if (Base.GetCooldown() && Base.GetCooldown().GrantedTags.Length > 0) 
                Owner.ApplyGameplayEffect(Owner.GenerateEffectSpec(this, Base.GetCooldown()));

            if (Base.GetCost() && Base.GetCost().ImpactSpecification.AttributeTarget) 
                Owner.ApplyGameplayEffect(Owner.GenerateEffectSpec(this, Base.GetCost()));
        }
            
        public bool ValidateActivationRequirements()
        {
            return !(GetCooldown().DurationRemaining > 0f)
                   && CanCoverCost()
                   && Base.GetTags().ValidateSourceRequirements(Owner);
        }

        public bool ValidateActivationRequirements(ITarget target)
        {
            return ValidateActivationRequirements()
                   && Base.GetTags().ValidateTargetRequirements(target);
        }

        public GameplayEffectDuration GetCooldown()
        {
            if (!Base.GetCooldown() || !(Base.GetCooldown().GrantedTags.Length > 0)) return default;
            return Owner.GetLongestDurationFor(Base.GetCooldown().GrantedTags);
        }

        public bool CanCoverCost()
        {
            if (!Base.GetCost() || !Base.GetCost().ImpactSpecification.AttributeTarget) return true;
            if (!Owner.FindAttributeSystem(out var attr) || !attr.TryGetAttributeValue(Base.GetCost().ImpactSpecification.AttributeTarget, out AttributeValue attributeValue)) return false;
            return attributeValue.CurrentValue >= Base.GetCost().ImpactSpecification.GetMagnitude(Owner.GenerateEffectSpec(this, Base.GetCost()));
        }

        public ISource GetOwner() => Owner;
        public List<GameplayTagScriptableObject> GetContextTags()
        {
            return Base.GetTags().ContextTags;
        }
        public GameplayTagScriptableObject GetAssetTag()
        {
            return Base.GetTags().AssetTag;
        }
        public int GetLevel() => Level;
        public void SetLevel(int level) => Level = level;
        public float GetRelativeLevel() => RelativeLevel;
        public string GetName() => Base.GetDefinition().Name;
        public GameplayTagScriptableObject GetAffiliation()
        {
            return Owner.GetAffiliation();
        }

        public override string ToString()
        {
            return Base.ToString();
        }
    }
}
