namespace FESGameplayAbilitySystem
{
    public struct AbilityImpactData
    {
        private AbilityImpactData(GASComponentBase target, IAttribute attribute, SourcedModifiedAttributeValue sourcedModifier, AttributeValue realImpact)
        {
            Target = target;
            Attribute = attribute;
            SourcedModifier = sourcedModifier;
            RealImpact = realImpact;
        }

        public GASComponentBase Target;
        public IAttribute Attribute;
        public SourcedModifiedAttributeValue SourcedModifier;
        public AttributeValue RealImpact;

        public static AbilityImpactData Generate(GASComponentBase target, IAttribute attribute, SourcedModifiedAttributeValue sourcedModifier, AttributeValue realImpact)
        {
            return new AbilityImpactData(target, attribute, sourcedModifier, realImpact);
        }

        public override string ToString()
        {
            return $"{SourcedModifier.Derivation.GetSource()} -> {Target} => {Attribute} ({RealImpact})";
        }
    }
}
