using System.Collections.Generic;
using System.Linq;

namespace FESGameplayAbilitySystem.Core.AttributeSystem.AttributeWorker.Core
{
    public class AttributeModifierRuleGroupScriptableObject : AbstractAttributeModifierRuleScriptableObject
    {
        public List<AbstractAttributeModifierRuleScriptableObject> Rules;
        
        public override ModifiedAttributeValue ApplyRule(AttributeSystemComponent system, ModifiedAttributeValue modifiedAttributeValue)
        {
            return Rules.Aggregate(modifiedAttributeValue, (current, rule) => rule.ApplyRule(system, current));
        }
    }
}
