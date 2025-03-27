using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public abstract class AbstractManyAttributeChangeEventScriptableObject : AbstractAttributeChangeEventScriptableObject
    {
        public List<AttributeScriptableObject> TargetAttributes;
        public ESeparateTogetherPolicy HandlingPolicy;

        public override List<AttributeScriptableObject> GetKeyAttributes()
        {
            return TargetAttributes;
        }
        public override List<AbstractAttributeChangeEventScriptableObject> GetValueChangeEvents()
        {
            return new List<AbstractAttributeChangeEventScriptableObject>() { this };
        }

        public override bool ValidateWorkFor(GASComponentBase system, ref Dictionary<AttributeScriptableObject, CachedAttributeValue> attributeCache, SourcedModifiedAttributeCache modifiedAttributeCache)
        {
            foreach (var attribute in TargetAttributes)
            {
                if (modifiedAttributeCache.AttributeIsActive(attribute)) return true;
            }

            return false;
        }
    }

    public enum ESeparateTogetherPolicy
    {
        Separate,
        Together
    }
}
