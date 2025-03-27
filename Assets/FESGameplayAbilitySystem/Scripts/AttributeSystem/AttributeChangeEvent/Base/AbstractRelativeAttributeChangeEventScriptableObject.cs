using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public abstract class AbstractRelativeAttributeChangeEventScriptableObject : AbstractFocusedAttributeChangeEventScriptableObject
    {
        [Header("Relative Attribute")]
        
        public AttributeScriptableObject RelativeTo;
        public float RelativeMultiplier = 1f;

        public override bool ValidateWorkFor(GASComponentBase system, ref Dictionary<AttributeScriptableObject, CachedAttributeValue> attributeCache, SourcedModifiedAttributeCache modifiedAttributeCache)
        {
            return attributeCache.ContainsKey(RelativeTo) && base.ValidateWorkFor(system, ref attributeCache, modifiedAttributeCache);
        }
    }
}
