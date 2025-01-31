using System;
using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(fileName = "ACE_DamageTypeSpecificScalarMultiplyRelative", menuName = "FESGAS/Authored/Attribute Change Event/Damage Type Specific Scalar Multiply Relative", order = 0)]
    public class DamageTypeSpecificScalarMultiplyRelativeAttributeChangeEvent : ScalarMultiplyRelativeAttributeChangeEvent
    {
        public EDamageType DamageType;
        
        public override void PreAttributeChange(GASComponent system, ref Dictionary<AttributeScriptableObject, AttributeValue> attributeCache, SourcedModifiedAttributeCache modifiedAttributeCache)
        {
            if (!modifiedAttributeCache.AttributeIsActive(PrimaryAttribute)) return;
            modifiedAttributeCache.Multiply(PrimaryAttribute, DamageType, SignPolicy, attributeCache[RelativeTo].CurrentValue * RelativeMultiplier, IsScalar, ClampScalar01);
        }
        
        public override void PostAttributeChange(GASComponent system, ref Dictionary<AttributeScriptableObject, AttributeValue> attributeCache, SourcedModifiedAttributeCache modifiedAttributeCache)
        {
            // Shouldn't implement anything here
        }
    }
}
    