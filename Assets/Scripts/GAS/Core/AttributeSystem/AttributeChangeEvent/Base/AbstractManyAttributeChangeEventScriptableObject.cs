using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public abstract class AbstractManyAttributeChangeEventScriptableObject : AbstractAttributeChangeEventScriptableObject
    {
        public List<AttributeScriptableObject> TargetAttributes;
        public ESeparateTogetherPolicy HandlingPolicy;
    }

    public enum ESeparateTogetherPolicy
    {
        Separate,
        Together
    }
}
