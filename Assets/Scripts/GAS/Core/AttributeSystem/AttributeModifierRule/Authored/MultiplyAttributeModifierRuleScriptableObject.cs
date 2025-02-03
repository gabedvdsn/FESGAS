using FESGameplayAbilitySystem.Core.AttributeSystem.AttributeWorker.Core;

namespace FESGameplayAbilitySystem.Core.AttributeSystem.AttributeModifierRule.Authored
{
    public class MultiplyAttributeModifierRuleScriptableObject : AbstractFocusedAttributeModifierRuleScriptableObject
    {
        public override ModifiedAttributeValue ApplyRule(AttributeSystemComponent system, ModifiedAttributeValue modifiedAttributeValue)
        {
            return !system.TryGetAttributeValue(SourceAttribute, out CachedAttributeValue attributeValue) ? modifiedAttributeValue : modifiedAttributeValue.Multiply(attributeValue.Value);
        }
    }
}
