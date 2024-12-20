using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    /// <summary>
    /// Clamps an attribute value based on the current and base value of another attribute
    /// </summary>
    [CreateAssetMenu(menuName = "FESGAS/Authored/Attribute Change Event/Clamp Relative", fileName = "New Clamp Relative Event")]
    public class ClampRelativeAttributeChangeEventScriptableObject : AbstractAttributeChangeEventScriptableObject
    {
        public AttributeScriptableObject MaxAttribute;
        
        public override void PreAttributeChange(GASComponent system, ref Dictionary<AttributeScriptableObject, AttributeValue> attributeCache, ref Dictionary<AttributeScriptableObject, ModifiedAttributeValue> modifiedAttributeCache)
        {
            // Clamp shouldn't implement anything here
        }
        
        public override void PostAttributeChange(GASComponent system, ref Dictionary<AttributeScriptableObject, AttributeValue> attributeCache, ref Dictionary<AttributeScriptableObject, ModifiedAttributeValue> modifiedAttributeCache)
        {
            // If the primary attribute was never modified, no need to check
            if (!modifiedAttributeCache.TryGetValue(PrimaryAttribute, out ModifiedAttributeValue modifiedAttributeValue)) return;
            
            // Assign the clamped updated attribute value directly
            AttributeValue maxAttributeValue = attributeCache[MaxAttribute];
            AttributeValue primaryAttributeValue = attributeCache[PrimaryAttribute];
            if (maxAttributeValue.CurrentValue < primaryAttributeValue.CurrentValue)
            {
                modifiedAttributeValue.DeltaCurrentValue -= primaryAttributeValue.CurrentValue - maxAttributeValue.CurrentValue;
                primaryAttributeValue.CurrentValue = maxAttributeValue.CurrentValue;
            }
            if (maxAttributeValue.BaseValue < primaryAttributeValue.BaseValue)
            {
                modifiedAttributeValue.DeltaBaseValue -= primaryAttributeValue.BaseValue - maxAttributeValue.BaseValue;
                primaryAttributeValue.BaseValue = maxAttributeValue.BaseValue;
            }

            attributeCache[PrimaryAttribute] = primaryAttributeValue;
            
            // Set the modified attribute value to reflect the actual modification post clamp
            modifiedAttributeCache[PrimaryAttribute] = modifiedAttributeValue;
        }
    }
}
