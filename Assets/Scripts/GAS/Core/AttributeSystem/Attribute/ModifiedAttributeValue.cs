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

        public AttributeValue ToAttributeValue() => new AttributeValue(DeltaCurrentValue, DeltaBaseValue);

        public override string ToString()
        {
            return $"[ MOD-ATTR ] {DeltaCurrentValue}/{DeltaBaseValue}";
        }
    }
}
