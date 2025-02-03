namespace FESGameplayAbilitySystem
{
    public interface IAttributeDerivation
    {
        public IEffectDerivation GetEffectDerivation();
        public GASComponent GetSource();
        public EImpactType GetImpactType();

        public static SourceAttributeDerivation GenerateSourceDerivation(GASComponent source, EImpactType impactType = EImpactType.NotApplicable)
        {
            return new SourceAttributeDerivation(source, impactType);
        }

        public static SourceAttributeDerivation GenerateSourceDerivation(SourcedModifiedAttributeValue sourceModifier)
        {
            return new SourceAttributeDerivation(sourceModifier.Derivation.GetSource(), sourceModifier.Derivation.GetImpactType());
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
    }
}
