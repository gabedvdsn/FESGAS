using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(fileName = "ACE_DamageTypeSpecificScalarMultiplyRelative", menuName = "FESGAS/Authored/Attribute Change Event/Damage Type Specific Scalar Multiply Relative", order = 0)]
    public class DamageTypeSpecificScalarMultiplyRelativeAttributeChangeEvent : ScalarMultiplyRelativeAttributeChangeEvent
    {
        [FormerlySerializedAs("DamageType")] public EImpactType impactType;
        
        public override void PreAttributeChange(GASComponent system, ref Dictionary<AttributeScriptableObject, CachedAttributeValue> attributeCache, SourcedModifiedAttributeCache modifiedAttributeCache)
        {
            if (!modifiedAttributeCache.AttributeIsActive(PrimaryAttribute)) return;
            modifiedAttributeCache.Multiply(PrimaryAttribute, impactType, SignPolicy, attributeCache[RelativeTo].Value.CurrentValue * RelativeMultiplier, IsScalar, ClampScalar01);
        }
        
        public override void PostAttributeChange(GASComponent system, ref Dictionary<AttributeScriptableObject, CachedAttributeValue> attributeCache, SourcedModifiedAttributeCache modifiedAttributeCache)
        {
            // Shouldn't implement anything here
        }
    }
}
    