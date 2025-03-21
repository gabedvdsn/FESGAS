using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace FESGameplayAbilitySystem
{
    /// <summary>
    /// Multiplies the SMAVs under the primary attribute by the current value
    /// </summary>
    [CreateAssetMenu(menuName = "FESGAS/Authored/Attribute Change Event/Multiply Relative", fileName = "ACE_MultiplyRelative")]
    public class MultiplyRelativeAttributeChangeEventScriptableObject : AbstractRelativeAttributeChangeEventScriptableObject
    {
        public ESignPolicy SignPolicy;
        
        public override void PreAttributeChange(GASComponent system, ref Dictionary<AttributeScriptableObject, CachedAttributeValue> attributeCache, SourcedModifiedAttributeCache modifiedAttributeCache)
        {
            if (!modifiedAttributeCache.AttributeIsActive(TargetAttribute)) return;
            modifiedAttributeCache.Multiply(TargetAttribute, attributeCache[RelativeTo].Value);
        }
        
        public override void PostAttributeChange(GASComponent system, ref Dictionary<AttributeScriptableObject, CachedAttributeValue> attributeCache, SourcedModifiedAttributeCache modifiedAttributeCache)
        {
            // Shouldn't implement anything here
        }
    }

    public enum ESignPolicy
    {
        Negative,
        Positive,
        ZeroBiased,
        ZeroNeutral
    }
}
