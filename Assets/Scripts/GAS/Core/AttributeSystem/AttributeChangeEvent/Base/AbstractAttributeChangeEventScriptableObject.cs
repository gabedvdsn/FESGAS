using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public abstract class AbstractAttributeChangeEventScriptableObject : ScriptableObject
    {
        public abstract void PreAttributeChange(GASComponent system, ref Dictionary<AttributeScriptableObject, AttributeValue> attributeCache, SourcedModifiedAttributeCache modifiedAttributeCache);
        public abstract void PostAttributeChange(GASComponent system, ref Dictionary<AttributeScriptableObject, AttributeValue> attributeCache, SourcedModifiedAttributeCache modifiedAttributeCache);
    }
}
