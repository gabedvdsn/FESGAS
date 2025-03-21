using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(fileName = "ACE_DamageTypeSpecificScalarMultiplyRelative", menuName = "FESGAS/Authored/Attribute Change Event/Damage Type Specific Scalar Multiply Relative", order = 0)]
    public class DamageTypeSpecificScalarMultiplyRelativeAttributeChangeEvent : ScalarMultiplyRelativeAttributeChangeEvent
    {
        public EImpactType ImpactType;
        
        public override void PreAttributeChange(GASComponent system, ref Dictionary<AttributeScriptableObject, CachedAttributeValue> attributeCache, SourcedModifiedAttributeCache modifiedAttributeCache)
        {
            if (!modifiedAttributeCache.AttributeIsActive(TargetAttribute)) return;
            if (!attributeCache.ContainsKey(RelativeTo)) return;
            modifiedAttributeCache.Multiply(TargetAttribute, ImpactType, SignPolicy, attributeCache[RelativeTo].Value.CurrentValue * RelativeMultiplier, IsModifier);
        }
        
        public override void PostAttributeChange(GASComponent system, ref Dictionary<AttributeScriptableObject, CachedAttributeValue> attributeCache, SourcedModifiedAttributeCache modifiedAttributeCache)
        {
            // Shouldn't implement anything here
        }
    }
}
    