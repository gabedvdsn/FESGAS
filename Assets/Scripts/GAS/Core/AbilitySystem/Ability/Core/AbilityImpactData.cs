namespace FESGameplayAbilitySystem
{
    public struct AbilityImpactData
    {
        private AbilityImpactData(GASComponent target, AttributeScriptableObject attribute, SourcedModifiedAttributeValue sourcedModifier, AttributeValue realImpact)
        {
            Target = target;
            Attribute = attribute;
            SourcedModifier = sourcedModifier;
            RealImpact = realImpact;
        }

        public GASComponent Target;
        public AttributeScriptableObject Attribute;
        public SourcedModifiedAttributeValue SourcedModifier;
        public AttributeValue RealImpact;

        public static AbilityImpactData Generate(GASComponent target, AttributeScriptableObject attribute, SourcedModifiedAttributeValue sourcedModifier, AttributeValue realImpact)
        {
            return new AbilityImpactData(target, attribute, sourcedModifier, realImpact);
        }

        public override string ToString()
        {
            return $"{SourcedModifier.Derivation.GetSource()} -> {Target} => {Attribute} ({RealImpact})";
        }
    }
}
