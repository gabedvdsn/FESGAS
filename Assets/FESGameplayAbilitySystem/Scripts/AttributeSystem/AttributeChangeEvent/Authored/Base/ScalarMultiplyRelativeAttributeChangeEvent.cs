using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(fileName = "ACE_ScalarMultiplyRelative", menuName = "FESGAS/Attribute/Change Event/Scalar Multiply Relative")]
    public class ScalarMultiplyRelativeAttributeChangeEvent : SignedMultiplyAttributeChangeEventScriptableObject
    {
        public override void PreAttributeChange(GASComponentBase system, ref Dictionary<AttributeScriptableObject, CachedAttributeValue> attributeCache,
            SourcedModifiedAttributeCache modifiedAttributeCache)
        {
            if (!modifiedAttributeCache.AttributeIsActive(TargetAttribute)) return;
            modifiedAttributeCache.Multiply(TargetAttribute, SignPolicy, attributeCache[RelativeTo].Value * RelativeMultiplier);
        }
        
        public override void PostAttributeChange(GASComponentBase system, ref Dictionary<AttributeScriptableObject, CachedAttributeValue> attributeCache,
            SourcedModifiedAttributeCache modifiedAttributeCache)
        {
            // Shouldn't implement anything here
        }
    }
}
