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
        public void RunEffectTickWorkers();
        public void RunEffectRemovalWorkers();
        public void RunEffectImpactWorkers(AbilityImpactData impactData);
        
        public static SourceAttributeDerivation GenerateSourceDerivation(GASComponentBase source, AttributeScriptableObject attribute, EImpactType impactType = EImpactType.NotApplicable, bool retainImpact = true)
        {
            return new SourceAttributeDerivation(source, attribute, impactType, retainImpact);
        }

        public static SourceAttributeDerivation GenerateSourceDerivation(SourcedModifiedAttributeValue sourceModifier, EImpactType impactType, bool retainImpact = true)
        {
            return GenerateSourceDerivation(sourceModifier.Derivation.GetSource(), sourceModifier.Derivation.GetAttribute(), impactType, retainImpact);
        }
    }

    public class SourceAttributeDerivation : IAttributeImpactDerivation
    {
        private GASComponentBase Source;
        public AttributeScriptableObject Attribute;
        private EImpactType ImpactType;
        private bool RetainImpact;

        public SourceAttributeDerivation(GASComponentBase source, AttributeScriptableObject attribute, EImpactType impactType, bool retainImpact = true)
        {
            Source = source;
            Attribute = attribute;
            ImpactType = impactType;
            RetainImpact = retainImpact;
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
            return RetainImpact;
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
        public void RunEffectTickWorkers()
        {
            // Nothing to do here!
        }
        public void RunEffectRemovalWorkers()
        {
            // Nothing to do here!
        }
        public void RunEffectImpactWorkers(AbilityImpactData impactData)
        {
            // Nothing to do here!
        }
    }
}
