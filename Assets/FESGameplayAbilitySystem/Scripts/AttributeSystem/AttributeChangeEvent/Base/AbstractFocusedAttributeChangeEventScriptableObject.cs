using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace FESGameplayAbilitySystem
{
    public abstract class AbstractFocusedAttributeChangeEventScriptableObject : AbstractAttributeChangeEventScriptableObject
    {
        public AttributeScriptableObject TargetAttribute;

        public override List<AttributeScriptableObject> GetKeyAttributes()
        {
            return new List<AttributeScriptableObject>() { TargetAttribute };
        }

        public override List<AbstractAttributeChangeEventScriptableObject> GetValueChangeEvents()
        {
            return new List<AbstractAttributeChangeEventScriptableObject>() { this };
        }

        public override bool ValidateWorkFor(GASComponentBase system, ref Dictionary<AttributeScriptableObject, CachedAttributeValue> attributeCache,
            SourcedModifiedAttributeCache modifiedAttributeCache)
        {
            return modifiedAttributeCache.AttributeIsActive(TargetAttribute);
        }
    }
}
