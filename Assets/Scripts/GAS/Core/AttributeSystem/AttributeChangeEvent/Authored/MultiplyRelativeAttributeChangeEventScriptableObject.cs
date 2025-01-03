using System;
using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(menuName = "FESGAS/Authored/Attribute Change Event/Multiply Relative", fileName = "ACE_MultiplyRelative")]
    public class MultiplyRelativeAttributeChangeEventScriptableObject : AbstractRelativeAttributeChangeEventScriptableObject
    {
        public SignPolicy SignPolicy;
        public bool OneMinus = true;
        
        public override void PreAttributeChange(GASComponent system, ref Dictionary<AttributeScriptableObject, AttributeValue> attributeCache, ref Dictionary<AttributeScriptableObject, ModifiedAttributeValue> modifiedAttributeCache)
        {
            if (!modifiedAttributeCache.TryGetValue(PrimaryAttribute, out ModifiedAttributeValue modifiedAttributeValue)) return;
            AttributeValue value = attributeCache[RelativeTo];
            if (SignPolicy != modifiedAttributeValue.SignPolicy) return;
            modifiedAttributeCache[PrimaryAttribute] = modifiedAttributeValue.Multiply(value, OneMinus);
        }
        
        public override void PostAttributeChange(GASComponent system, ref Dictionary<AttributeScriptableObject, AttributeValue> attributeCache, ref Dictionary<AttributeScriptableObject, ModifiedAttributeValue> modifiedAttributeCache)
        {
            // Shouldn't implement anything here
        }
    }

    public enum SignPolicy
    {
        Negative,
        Positive,
        Both,
        Neither
    }
}
