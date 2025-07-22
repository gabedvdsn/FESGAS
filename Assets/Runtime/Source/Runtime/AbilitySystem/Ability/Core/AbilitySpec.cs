using System;
using System.Collections.Generic;
using System.Linq;

namespace FESGameplayAbilitySystem
{
    public class AbilitySpec : IEffectDerivation
    {
        public GASComponentBase Owner;
        public AbilityScriptableObject Base;
        public int Level;
        public float RelativeLevel => (Level - 1) / (float)(Base.MaxLevel - 1);
        
        private AbilitySpec(GASComponentBase owner, AbilityScriptableObject ability, int level)
        {
            Owner = owner;
            Base = ability;
            Level = level;
        }

        public static AbilitySpec Generate(AbilityScriptableObject ability, GASComponentBase owner, int level = 1)
        {
            return new AbilitySpec(owner, ability, level);
        }

        public void ApplyUsageEffects()
        {
            if (!Base.Cooldown || !(Base.Cooldown.GrantedTags.Length > 0)) return;
            if (!Base.Cost || !Base.Cost.ImpactSpecification.AttributeTarget) return;

            // Apply cost and cooldown effects
            Owner.ApplyGameplayEffect(Owner.GenerateEffectSpec(this, Base.Cooldown));
            Owner.ApplyGameplayEffect(Owner.GenerateEffectSpec(this, Base.Cost));
        }
            
        public bool ValidateActivationRequirements()
        {
            return !(GetCooldown().DurationRemaining > 0f)
                   && CanCoverCost()
                   && Base.Tags.ValidateSourceRequirements(Owner);
        }

        public bool ValidateActivationRequirements(GASComponentBase target)
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
            if (!Owner.FindAttributeSystem(out var attr) || !attr.TryGetAttributeValue(Base.Cost.ImpactSpecification.AttributeTarget, out AttributeValue attributeValue)) return false;
            return attributeValue.CurrentValue >= Base.Cost.ImpactSpecification.GetMagnitude(Owner.GenerateEffectSpec(this, Base.Cost));
        }

        public ISource GetOwner() => Owner;
        public List<GameplayTagScriptableObject> GetContextTags()
        {
            return Base.Tags.ContextTags;
        }
        public GameplayTagScriptableObject GetAssetTag()
        {
            return Base.Tags.AssetTag;
        }
        public int GetLevel() => Level;
        public void SetLevel(int level) => Level = level;
        public float GetRelativeLevel() => RelativeLevel;
        public string GetName() => Base.Definition.Name;
        public GameplayTagScriptableObject GetAffiliation()
        {
            return Owner.Identity.Affiliation;
        }

        public override string ToString()
        {
            return Base.ToString();
        }
    }
}
