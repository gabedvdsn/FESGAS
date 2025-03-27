using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace FESGameplayAbilitySystem
{
    public abstract class AbstractModifyContextAttributeChangeEvent : ScalarMultiplyRelativeAttributeChangeEvent
    {
        [FormerlySerializedAs("ImpactType")] [Header("Impact Context")]
        
        public EImpactType ModifyType;
        public EEffectImpactTarget ModifyTarget;
        public bool AllowSelfModification;
        
        [Space(5)]
        
        public bool AnyContextTag;
        [Tooltip("The modification source context tag must exist in this list")]
        public List<GameplayTagScriptableObject> ApplyAbilityContextTags;

        protected List<SourcedModifiedAttributeValue> GetApplicableValues(SourcedModifiedAttributeCache modifiedAttributeCache)
        {
            var usable = new List<SourcedModifiedAttributeValue>();
            if (!modifiedAttributeCache.TryGetSourcedModifiers(TargetAttribute, out var smavs)) return usable;

            foreach (var smav in smavs)
            {
                if (smav.BaseDerivation.GetImpactType() != ModifyType) continue;
                if (smav.SignPolicy != SignPolicy) continue;
                
                
            }
        }
    }
}
    