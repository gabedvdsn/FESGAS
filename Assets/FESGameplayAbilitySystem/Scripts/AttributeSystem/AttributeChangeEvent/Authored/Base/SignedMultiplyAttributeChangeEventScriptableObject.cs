using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace FESGameplayAbilitySystem
{
    /// <summary>
    /// Multiplies the SMAVs under the primary attribute by the current value
    /// </summary>
    [CreateAssetMenu(menuName = "FESGAS//Attribute/Change Event/Multiply Relative", fileName = "ACE_MultiplyRelative")]
    public class SignedMultiplyAttributeChangeEventScriptableObject : AbstractRelativeAttributeChangeEventScriptableObject
    {
        [Header("Signed Multiplier")]
        
        public ESignPolicy SignPolicy;
        
        public override void PreAttributeChange(GASComponentBase system, ref Dictionary<AttributeScriptableObject, CachedAttributeValue> attributeCache,
            SourcedModifiedAttributeCache modifiedAttributeCache)
        {
            if (!modifiedAttributeCache.AttributeIsActive(TargetAttribute)) return;
            modifiedAttributeCache.Multiply(TargetAttribute, SignPolicy, attributeCache[RelativeTo].Value);
        }
        
        public override void PostAttributeChange(GASComponentBase system, ref Dictionary<AttributeScriptableObject, CachedAttributeValue> attributeCache,
            SourcedModifiedAttributeCache modifiedAttributeCache)
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
