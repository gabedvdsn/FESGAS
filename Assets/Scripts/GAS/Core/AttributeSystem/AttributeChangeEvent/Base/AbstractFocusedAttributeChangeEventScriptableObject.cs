using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace FESGameplayAbilitySystem
{
    public abstract class AbstractFocusedAttributeChangeEventScriptableObject : AbstractAttributeChangeEventScriptableObject
    {
        public AttributeScriptableObject TargetAttribute;
    }
}
