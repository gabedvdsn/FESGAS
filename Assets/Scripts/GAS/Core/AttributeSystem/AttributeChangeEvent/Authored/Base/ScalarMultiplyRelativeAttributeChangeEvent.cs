using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(fileName = "ACE_ScalarMultiplyRelative", menuName = "FESGAS/Authored/Attribute Change Event/Scalar Multiply Relative", order = 0)]
    public class ScalarMultiplyRelativeAttributeChangeEvent : MultiplyRelativeAttributeChangeEventScriptableObject
    {
        [Tooltip("Is the relative value a scalar? (e.g. damage resistance or magic amplification")] 
        public bool IsScalar = true;
        [Tooltip("If the value is a scalar, should it be clamped between 0-1?")]
        public bool ClampScalar01 = true;
        
        public override void PreAttributeChange(GASComponent system, ref Dictionary<AttributeScriptableObject, CachedAttributeValue> attributeCache, SourcedModifiedAttributeCache modifiedAttributeCache)
        {
            if (!modifiedAttributeCache.AttributeIsActive(PrimaryAttribute)) return;
            modifiedAttributeCache.Multiply(PrimaryAttribute, SignPolicy, attributeCache[RelativeTo].Value.CurrentValue * RelativeMultiplier, IsScalar, ClampScalar01);
        }
        
        public override void PostAttributeChange(GASComponent system, ref Dictionary<AttributeScriptableObject, CachedAttributeValue> attributeCache, SourcedModifiedAttributeCache modifiedAttributeCache)
        {
            // Shouldn't implement anything here
        }
    }
}
