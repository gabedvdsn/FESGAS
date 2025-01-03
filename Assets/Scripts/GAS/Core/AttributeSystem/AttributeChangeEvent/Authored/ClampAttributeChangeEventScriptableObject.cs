using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    /// <summary>
    /// Clamps an attributes current value based on its base value
    /// </summary>
    [CreateAssetMenu(menuName = "FESGAS/Authored/Attribute Change Event/Clamp", fileName = "ACE_Clamp")]
    public class ClampAttributeChangeEventScriptableObject : AbstractFocusedAttributeChangeEventScriptableObject
    {
        public override void PreAttributeChange(GASComponent system, ref Dictionary<AttributeScriptableObject, AttributeValue> attributeCache, ref Dictionary<AttributeScriptableObject, ModifiedAttributeValue> modifiedAttributeCache)
        {
            // Clamp shouldn't implement anything here
        }
        
        public override void PostAttributeChange(GASComponent system, ref Dictionary<AttributeScriptableObject, AttributeValue> attributeCache, ref Dictionary<AttributeScriptableObject, ModifiedAttributeValue> modifiedAttributeCache)
        {
            // If the primary attribute was never modified, no need to check
            if (!modifiedAttributeCache.TryGetValue(PrimaryAttribute, out ModifiedAttributeValue modifiedAttributeValue)) return;
            
            // Assign the clamped updated attribute value directly
            AttributeValue primaryAttributeValue = attributeCache[PrimaryAttribute];
            if (primaryAttributeValue.BaseValue < primaryAttributeValue.CurrentValue)
            {
                modifiedAttributeValue.DeltaCurrentValue -= primaryAttributeValue.CurrentValue - primaryAttributeValue.BaseValue;
                primaryAttributeValue.CurrentValue = primaryAttributeValue.BaseValue;
            }

            attributeCache[PrimaryAttribute] = primaryAttributeValue;
            
            // Set the modified attribute value to reflect the actual modification post clamp
            modifiedAttributeCache[PrimaryAttribute] = modifiedAttributeValue;
        }
    }
}
