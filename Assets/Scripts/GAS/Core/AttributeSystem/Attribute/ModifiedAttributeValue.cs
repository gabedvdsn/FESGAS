namespace FESGameplayAbilitySystem
{
    public struct ModifiedAttributeValue
    {
        public float DeltaCurrentValue;
        public float DeltaBaseValue;

        public ModifiedAttributeValue(float deltaCurrentValue, float deltaBaseValue)
        {
            DeltaCurrentValue = deltaCurrentValue;
            DeltaBaseValue = deltaBaseValue;
        }

        public ModifiedAttributeValue Combine(ModifiedAttributeValue other)
        {
            return new ModifiedAttributeValue(
                DeltaCurrentValue + other.DeltaCurrentValue, 
                DeltaBaseValue + other.DeltaBaseValue);
        }

        public ModifiedAttributeValue Negate()
        {
            return new ModifiedAttributeValue(
                -DeltaCurrentValue,
                -DeltaBaseValue
            );
        }

        public SourcedModifiedAttributeValue ToSourced(AbilitySpec ability)
        {
            return new SourcedModifiedAttributeValue(
                ability,
                DeltaCurrentValue,
                DeltaBaseValue
            );
        }

        public AttributeValue ToAttributeValue() => new(DeltaCurrentValue, DeltaBaseValue);

        public override string ToString()
        {
            return $"[ MOD-ATTR ] {DeltaCurrentValue}/{DeltaBaseValue}";
        }
    }

    public struct SourcedModifiedAttributeValue
    {
        public AbilitySpec Ability;
        public float DeltaCurrentValue;
        public float DeltaBaseValue;

        public SourcedModifiedAttributeValue(AbilitySpec ability, float deltaCurrentValue, float deltaBaseValue)
        {
            Ability = ability;
            DeltaCurrentValue = deltaCurrentValue;
            DeltaBaseValue = deltaBaseValue;
        }

        public SourcedModifiedAttributeValue Combine(SourcedModifiedAttributeValue other)
        {
            if (other.Ability != Ability)
            {
                return this;
            }
            
            return new SourcedModifiedAttributeValue(
                Ability,
                DeltaCurrentValue + other.DeltaCurrentValue, 
                DeltaBaseValue + other.DeltaBaseValue
            );
        }

        public SourcedModifiedAttributeValue Negate()
        {
            return new SourcedModifiedAttributeValue(
                Ability,
                -DeltaCurrentValue,
                -DeltaBaseValue
            );
        }

        public ModifiedAttributeValue ToModifiedAttributeValue() => new(DeltaCurrentValue, DeltaBaseValue);

        public AttributeValue ToAttributeValue() => new(DeltaCurrentValue, DeltaBaseValue);

        public override string ToString()
        {
            return $"[ MOD-ATTR ] {DeltaCurrentValue}/{DeltaBaseValue}";
        }
    }
}
