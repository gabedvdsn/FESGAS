using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace FESGameplayAbilitySystem
{
    public abstract class AbstractFocusedAttributeChangeEventScriptableObject : AbstractAttributeChangeEventScriptableObject
    {
        [Header("Change Event")]
        
        public EChangeEventTiming Timing;

        [Header("Change Attribute Validation")] 
        
        public AttributeScriptableObject TargetAttribute;
        public EEffectImpactTargetExpanded TargetModification;
        public bool ExclusivelyTargetModification;
        public bool AllowSelfModification;
        
        [Space(5)]
        
        public ESignPolicyExtended SignPolicy;
        public EImpactTypeAny ImpactType;
        
        [Header("Change Tag Validation")]
        
        public bool AnyContextTag;
        [Tooltip("The modification source context tag(s) (all of them) must exist in this list")]
        public List<GameplayTagScriptableObject> ValidContextTags;

        public override bool ValidateWorkFor(GASComponentBase system, Dictionary<AttributeScriptableObject, CachedAttributeValue> attributeCache,
            ChangeValue change)
        {
            if (change.Value.BaseDerivation.GetAttribute() != TargetAttribute) return false;
            if (!AnyContextTag && ValidContextTags.Count > 0 && !ValidContextTags.ContainsAll(change.Value.BaseDerivation.GetContextTags())) return false;
            if (!AllowSelfModification && change.Value.BaseDerivation.GetSource() == system) return false;
            if (!GASHelper.ValidateImpactTargets(TargetModification, change.Value.ToAttributeValue(), ExclusivelyTargetModification)) return false;
            if (!GASHelper.ValidateSignPolicy(SignPolicy, TargetModification, change.Value.ToAttributeValue())) return false;
            if (!GASHelper.ValidateImpactTypes(change.Value.BaseDerivation.GetImpactType(), ImpactType)) return false;
            return true;
        }

        public override bool RegisterWithHandler(AttributeChangeMomentHandler preChange, AttributeChangeMomentHandler postChange)
        {
            return Timing switch
            {
                EChangeEventTiming.PreChange => preChange.AddEvent(TargetAttribute, this),
                EChangeEventTiming.PostChange => postChange.AddEvent(TargetAttribute, this),
                EChangeEventTiming.Both => preChange.AddEvent(TargetAttribute, this) || postChange.AddEvent(TargetAttribute, this),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public override bool DeRegisterFromHandler(AttributeChangeMomentHandler preChange, AttributeChangeMomentHandler postChange)
        {
            return Timing switch
            {
                EChangeEventTiming.PreChange => preChange.RemoveEvent(TargetAttribute, this),
                EChangeEventTiming.PostChange => postChange.RemoveEvent(TargetAttribute, this),
                EChangeEventTiming.Both => preChange.RemoveEvent(TargetAttribute, this) || postChange.RemoveEvent(TargetAttribute, this),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
