using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public interface IAttributeImpactDerivation
    {
        public AttributeScriptableObject GetAttribute();
        public IEffectDerivation GetEffectDerivation();
        public GASComponentBase GetSource();
        public GASComponentBase GetTarget();
        public EImpactType GetImpactType();
        public bool RetainAttributeImpact();
        public void TrackImpact(AbilityImpactData impactData);
        public bool TryGetTrackedImpact(out AttributeValue impactValue);
        public bool TryGetLastTrackedImpact(out AttributeValue impactValue);
        public List<GameplayTagScriptableObject> GetContextTags();
        public void RunEffectApplicationWorkers();
        public void RunEffectRemovalWorkers();
        public void RunEffectWorkers(AbilityImpactData impactData);
        
        public static SourceAttributeDerivation GenerateSourceDerivation(GASComponentBase source, AttributeScriptableObject attribute, EImpactType impactType = EImpactType.NotApplicable)
        {
            return new SourceAttributeDerivation(source, attribute, impactType);
        }

        public static SourceAttributeDerivation GenerateSourceDerivation(SourcedModifiedAttributeValue sourceModifier, EImpactType impactType)
        {
            return GenerateSourceDerivation(sourceModifier.Derivation.GetSource(), sourceModifier.Derivation.GetAttribute(), impactType);
        }
    }

    public class SourceAttributeDerivation : IAttributeImpactDerivation
    {
        private GASComponentBase Source;
        public AttributeScriptableObject Attribute;
        private EImpactType ImpactType;

        public SourceAttributeDerivation(GASComponentBase source, AttributeScriptableObject attribute, EImpactType impactType)
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
        public GASComponentBase GetSource()
        {
            return Source;
        }
        public GASComponentBase GetTarget()
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
        
        public void TrackImpact(AbilityImpactData impactData)
        {
            // Source derivations do not track their impact
        }
        
        public bool TryGetTrackedImpact(out AttributeValue impactValue)
        {
            impactValue = default;
            return false;
        }
        public bool TryGetLastTrackedImpact(out AttributeValue impactValue)
        {
            impactValue = default;
            return false;
        }
        public List<GameplayTagScriptableObject> GetContextTags()
        {
            return new List<GameplayTagScriptableObject>() { Source.Identity.NameTag };
        }
        public void RunEffectApplicationWorkers()
        {
            // Nothing to do here!
        }
        public void RunEffectRemovalWorkers()
        {
            // Nothing to do here!
        }
        public void RunEffectWorkers(AbilityImpactData impactData)
        {
            // Nothing to do here!
        }
    }
}
