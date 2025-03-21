using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(fileName = "ACE_ScalarMultiplyRelative", menuName = "FESGAS/Authored/Attribute Change Event/Scalar Multiply Relative", order = 0)]
    public class ScalarMultiplyRelativeAttributeChangeEvent : MultiplyRelativeAttributeChangeEventScriptableObject
    {
        [Tooltip("Is the relative value a modifier? (e.g. damage resistance or magic amplification")] 
        public bool IsModifier = true;
        
        public override void PreAttributeChange(GASComponent system, ref Dictionary<AttributeScriptableObject, CachedAttributeValue> attributeCache, SourcedModifiedAttributeCache modifiedAttributeCache)
        {
            if (!modifiedAttributeCache.AttributeIsActive(TargetAttribute)) return;
            modifiedAttributeCache.Multiply(TargetAttribute, SignPolicy, attributeCache[RelativeTo].Value.CurrentValue * RelativeMultiplier, IsModifier);
        }
        
        public override void PostAttributeChange(GASComponent system, ref Dictionary<AttributeScriptableObject, CachedAttributeValue> attributeCache, SourcedModifiedAttributeCache modifiedAttributeCache)
        {
            // Shouldn't implement anything here
        }
    }
}
