using UnityEngine;

namespace FESGameplayAbilitySystem.Core.AttributeSystem.AttributeWorker.Core
{
    /// <summary>
    /// Useful for applying
    /// </summary>
    public abstract class AbstractAttributeModifierRuleScriptableObject : ScriptableObject
    {
        public abstract ModifiedAttributeValue ApplyRule(AttributeSystemComponent system, ModifiedAttributeValue modifiedAttributeValue);
    }
}
