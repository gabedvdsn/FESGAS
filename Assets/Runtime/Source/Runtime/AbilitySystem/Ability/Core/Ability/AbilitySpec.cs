using System;
using System.Collections.Generic;
using System.Linq;

namespace FESGameplayAbilitySystem
{
    public class AbilitySpec : IEffectOrigin
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
            if (Base.GetCooldown() is not null && Base.GetCooldown().Tags.GrantedTags.Length > 0) 
                Owner.ApplyGameplayEffect(Owner.GenerateEffectSpec(this, Base.GetCooldown()));

            if (Base.GetCost() is not null && Base.GetCost().ImpactSpecification.AttributeTarget.valid) 
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
            if (Base.GetCooldown() is null || !(Base.GetCooldown().Tags.GrantedTags.Length > 0)) return default;
            return Owner.GetLongestDurationFor(Base.GetCooldown().Tags.GrantedTags);
        }

        public bool CanCoverCost()
        {
            if (Base.GetCost() is null || !Base.GetCost().ImpactSpecification.AttributeTarget.valid) return true;
            if (!Owner.FindAttributeSystem(out var attr) || !attr.TryGetAttributeValue(Base.GetCost().ImpactSpecification.AttributeTarget, out AttributeValue attributeValue)) return false;
            return attributeValue.CurrentValue >= Base.GetCost().ImpactSpecification.GetMagnitude(Owner.GenerateEffectSpec(this, Base.GetCost()));
        }

        public ISource GetOwner() => Owner;
        public Tag[] GetContextTags()
        {
            return Base.GetTags().ContextTags;
        }
        public Tag GetAssetTag()
        {
            return Base.GetTags().AssetTag;
        }
        public int GetLevel() => Level;
        public void SetLevel(int level) => Level = level;
        public float GetRelativeLevel() => RelativeLevel;
        public string GetName() => Base.GetDefinition().Name;
        public Tag GetAffiliation()
        {
            return Owner.GetAffiliation();
        }

        public override string ToString()
        {
            return Base.ToString();
        }
    }
}
