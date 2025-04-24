using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public abstract class AbstractRelativeAttributeChangeEventScriptableObject : AbstractFocusedAttributeChangeEventScriptableObject
    {
        [Header("Relative Attribute Event")]
        
        public AttributeScriptableObject RelativeTo;
        public float RelativeMultiplier = 1f;

        public override bool ValidateWorkFor(GASComponentBase system, Dictionary<AttributeScriptableObject, CachedAttributeValue> attributeCache,
            ChangeValue change)
        {
            return attributeCache.ContainsKey(RelativeTo) && base.ValidateWorkFor(system, attributeCache, change);
        }
    }
}
