﻿using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(fileName = "ACE_DamageAmp_", menuName = "FESGAS/Attribute/Change Event/Damage Amplification")]
    public class DamageAmplificationAttributeChangeEvent : AbstractModifyContextAttributeChangeEvent
    {
        public override void PreAttributeChange(GASComponentBase system, ref Dictionary<AttributeScriptableObject, CachedAttributeValue> attributeCache,
            SourcedModifiedAttributeCache modifiedAttributeCache)
        {
            if (!modifiedAttributeCache.AttributeIsActive(TargetAttribute)) return;
            if (!attributeCache.ContainsKey(RelativeTo)) return;
            modifiedAttributeCache.MultiplyAmplify(TargetAttribute, ModifyType, SignPolicy, attributeCache[RelativeTo].Value * RelativeMultiplier);
        }
        
        public override void PostAttributeChange(GASComponentBase system, ref Dictionary<AttributeScriptableObject, CachedAttributeValue> attributeCache,
            SourcedModifiedAttributeCache modifiedAttributeCache)
        {
            // Shouldn't implement anything here
        }
    }
}
