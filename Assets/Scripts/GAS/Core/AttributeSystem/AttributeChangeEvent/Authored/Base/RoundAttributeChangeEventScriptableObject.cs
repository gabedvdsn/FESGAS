using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public class RoundAttributeChangeEventScriptableObject : AbstractManyAttributeChangeEventScriptableObject
    {
        public override void PreAttributeChange(GASComponent system, ref Dictionary<AttributeScriptableObject, CachedAttributeValue> attributeCache, SourcedModifiedAttributeCache modifiedAttributeCache)
        {
            // Doesn't implement anything here
        }
        
        public override void PostAttributeChange(GASComponent system, ref Dictionary<AttributeScriptableObject, CachedAttributeValue> attributeCache, SourcedModifiedAttributeCache modifiedAttributeCache)
        {
            if (HandlingPolicy == ESeparateTogetherPolicy.Separate)
            {
                foreach (AttributeScriptableObject attribute in TargetAttributes)
                {
                    if (!modifiedAttributeCache.AttributeIsActive(attribute)) continue;
                    
                }
            }
        }
    }

    public enum ERoundPolicy
    {
        Round,
        Ceil,
        Floor
    }
}
