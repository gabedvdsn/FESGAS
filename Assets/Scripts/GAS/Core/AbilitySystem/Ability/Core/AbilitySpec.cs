namespace FESGameplayAbilitySystem
{
    public class AbilitySpec : IModifiesAttributes
    {
        public GASComponent Owner;
        public AbilityScriptableObject Base;
        public int Level;
        public float RelativeLevel => (Level - 1) / (float)(Base.MaxLevel - 1);
        
        public AbilitySpec(GASComponent owner, AbilityScriptableObject ability, int level)
        {
            Owner = owner;
            Base = ability;
            Level = level;
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
            return attributeValue.CurrentValue >= Base.Cost.ImpactSpecification.GetMagnitude(Owner.GenerateEffectSpec(this, Base.Cost));
        }
    }
}
