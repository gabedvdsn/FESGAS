namespace FESGameplayAbilitySystem
{
    public struct AbilityImpactData
    {
        private AbilityImpactData(AttributeScriptableObject attribute, SourcedModifiedAttributeValue sourcedModifier, AttributeValue realImpact)
        {
            Attribute = attribute;
            SourcedModifier = sourcedModifier;
            RealImpact = realImpact;
        }

        public AttributeScriptableObject Attribute;
        public SourcedModifiedAttributeValue SourcedModifier;
        public AttributeValue RealImpact;

        public static AbilityImpactData Generate(AttributeScriptableObject attribute, SourcedModifiedAttributeValue sourcedModifier, AttributeValue realImpact)
        {
            return new AbilityImpactData(attribute, sourcedModifier, realImpact);
        }

        public override string ToString()
        {
            return $"{Attribute} ({RealImpact})";
        }
    }
}
