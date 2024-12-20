using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    /// <summary>
    /// Scales an attribute value to match the change in another attribute value. Scales both current and base values.
    /// <example>Max health is changed, scale Health to retain the same proportion to Max Health</example>
    /// </summary>
    [CreateAssetMenu(menuName = "FESGAS/Authored/Attribute Change Event/Scale Relative", fileName = "New Scale Relative Event")]
    public class ScaleRelativeAttributeChangeEventScriptableObject : AbstractAttributeChangeEventScriptableObject
    {
        public AttributeScriptableObject RelativeAttribute;
        
        public override void PreAttributeChange(GASComponent system, ref Dictionary<AttributeScriptableObject, AttributeValue> attributeCache, ref Dictionary<AttributeScriptableObject, ModifiedAttributeValue> modifiedAttributeCache)
        {
            // Scale shouldn't implement anything here
        }
        
        public override void PostAttributeChange(GASComponent system, ref Dictionary<AttributeScriptableObject, AttributeValue> attributeCache, ref Dictionary<AttributeScriptableObject, ModifiedAttributeValue> modifiedAttributeCache)
        {
            if (!modifiedAttributeCache.TryGetValue(PrimaryAttribute, out ModifiedAttributeValue modifiedAttributeValue)) return;

            float currProportion = attributeCache[RelativeAttribute].CurrentValue / attributeCache[PrimaryAttribute].CurrentValue;
            float baseProportion = attributeCache[RelativeAttribute].BaseValue / attributeCache[PrimaryAttribute].BaseValue;
            attributeCache[RelativeAttribute] = new AttributeValue(
                attributeCache[PrimaryAttribute].CurrentValue * currProportion,
                attributeCache[PrimaryAttribute].BaseValue * baseProportion
            );
        }
    }
}
