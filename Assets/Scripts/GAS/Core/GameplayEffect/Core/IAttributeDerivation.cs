namespace FESGameplayAbilitySystem
{
    public interface IAttributeDerivation
    {
        public IEffectDerivation GetEffectDerivation();
        public GASComponent GetSource();
        public EImpactType GetImpactType();
        public bool RetainAttributeImpact();
        
        public static SourceAttributeDerivation GenerateSourceDerivation(GASComponent source, EImpactType impactType = EImpactType.NotApplicable)
        {
            return new SourceAttributeDerivation(source, impactType);
        }

        public static SourceAttributeDerivation GenerateSourceDerivation(SourcedModifiedAttributeValue sourceModifier, EImpactType impactType)
        {
            return GenerateSourceDerivation(sourceModifier.Derivation.GetSource(), impactType);
        }
    }

    public class SourceAttributeDerivation : IAttributeDerivation
    {
        private GASComponent Source;
        private EImpactType ImpactType;

        public SourceAttributeDerivation(GASComponent source, EImpactType impactType)
        {
            Source = source;
            ImpactType = impactType;
        }

        public IEffectDerivation GetEffectDerivation()
        {
            return IEffectDerivation.GenerateSourceDerivation(Source);
        }
        public GASComponent GetSource()
        {
            return Source;
        }
        public EImpactType GetImpactType()
        {
            return ImpactType;
        }

        public bool RetainAttributeImpact()
        {
            return true;
        }
    }
}
