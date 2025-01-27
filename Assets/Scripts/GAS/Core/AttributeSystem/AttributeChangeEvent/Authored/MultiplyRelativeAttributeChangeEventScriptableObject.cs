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
        
        public override void PreAttributeChange(GASComponent system, ref Dictionary<AttributeScriptableObject, AttributeValue> attributeCache, SourcedModifiedAttributeCache modifiedAttributeCache)
        {
            if (!modifiedAttributeCache.TryToModified(PrimaryAttribute, out ModifiedAttributeValue modifiedAttributeValue)) return;
            if (SignPolicy != modifiedAttributeValue.SignPolicy) return;
            
            modifiedAttributeCache.Set(PrimaryAttribute, modifiedAttributeValue.Multiply(attributeCache[RelativeTo], OneMinus));
            modifiedAttributeCache.Multiply(PrimaryAttribute, modifiedAttributeValue.Multiply(attributeCache[RelativeTo], OneMinus));
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
