using System.Collections.Generic;

namespace FESGameplayAbilitySystem
{
    public abstract class AbstractRelativeAttributeChangeEventScriptableObject : AbstractFocusedAttributeChangeEventScriptableObject
    {
        public AttributeScriptableObject RelativeTo;
        public float RelativeMultiplier = 1f;
    }
}
