using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    /// <summary>
    /// Attribute impact derivations are sources of attribute impact (impact carriers)
    /// </summary>
    public interface IAttributeImpactDerivation
    {
        public Attribute GetAttribute();
        public IEffectOrigin GetEffectDerivation();
        public ISource GetSource();
        public ITarget GetTarget();
        public EImpactType GetImpactType();
        public Tag AttributeRetention();
        public void TrackImpact(AbilityImpactData impactData);
        public bool TryGetTrackedImpact(out AttributeValue impactValue);
        public bool TryGetLastTrackedImpact(out AttributeValue impactValue);
        public Tag[] GetContextTags();
        public void RunEffectApplicationWorkers();
        public void RunEffectTickWorkers();
        public void RunEffectRemovalWorkers();
        public void RunEffectImpactWorkers(AbilityImpactData impactData);
        public Dictionary<IMagnitudeModifier, AttributeValue?> GetSourcedCapturedAttributes();
        
        public static SourceAttributeDerivation GenerateSourceDerivation(ISource source, Attribute attribute, EImpactType impactType = EImpactType.NotApplicable, bool retainImpact = true)
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
        private ISource Source;
        public Attribute Attribute;
        private EImpactType ImpactType;
        private bool RetainImpact;

        public SourceAttributeDerivation(ISource source, Attribute attribute, EImpactType impactType, bool retainImpact = true)
        {
            Source = source;
            Attribute = attribute;
            ImpactType = impactType;
            RetainImpact = retainImpact;
        }

        public Attribute GetAttribute()
        {
            return Attribute;
        }
        public IEffectOrigin GetEffectDerivation()
        {
            return IEffectOrigin.GenerateSourceDerivation(Source);
        }
        public ISource GetSource()
        {
            return Source;
        }
        public ITarget GetTarget()
        {
            return Source;
        }
        public EImpactType GetImpactType()
        {
            return ImpactType;
        }

        public Tag AttributeRetention()
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
        public Tag[] GetContextTags()
        {
            return Source.GetContextTags();
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
        public Dictionary<IMagnitudeModifier, AttributeValue?> GetSourcedCapturedAttributes()
        {
            return new();
        }
    }
}
