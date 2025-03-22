using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public interface IAttributeDerivation
    {
        public AttributeScriptableObject GetAttribute();
        public IEffectDerivation GetEffectDerivation();
        public GASComponent GetSource();
        public EImpactType GetImpactType();
        public bool RetainAttributeImpact();
        public void TrackImpact(AttributeValue impactValue);
        public bool TryGetTrackedImpact(out AttributeValue impactValue);
        
        public static SourceAttributeDerivation GenerateSourceDerivation(GASComponent source, AttributeScriptableObject attribute, EImpactType impactType = EImpactType.NotApplicable)
        {
            return new SourceAttributeDerivation(source, attribute, impactType);
        }

        public static SourceAttributeDerivation GenerateSourceDerivation(SourcedModifiedAttributeValue sourceModifier, EImpactType impactType)
        {
            return GenerateSourceDerivation(sourceModifier.Derivation.GetSource(), sourceModifier.Derivation.GetAttribute(), impactType);
        }
    }

    public class SourceAttributeDerivation : IAttributeDerivation
    {
        private GASComponent Source;
        public AttributeScriptableObject Attribute;
        private EImpactType ImpactType;

        public SourceAttributeDerivation(GASComponent source, AttributeScriptableObject attribute, EImpactType impactType)
        {
            Source = source;
            Attribute = attribute;
            ImpactType = impactType;
        }

        public AttributeScriptableObject GetAttribute()
        {
            return Attribute;
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
        
        public void TrackImpact(AttributeValue impactValue)
        {
            // Source derivations do not track their impact
        }
        
        public bool TryGetTrackedImpact(out AttributeValue impactValue)
        {
            impactValue = default;
            return false;
        }
    }
}
