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
        public override void PreAttributeChange(GASComponent system, ref Dictionary<AttributeScriptableObject, CachedAttributeValue> attributeCache, SourcedModifiedAttributeCache modifiedAttributeCache)
        {
            // Clamp shouldn't implement anything here
        }
        
        public override void PostAttributeChange(GASComponent system, ref Dictionary<AttributeScriptableObject, CachedAttributeValue> attributeCache, SourcedModifiedAttributeCache modifiedAttributeCache)
        {
            // If the primary attribute was never modified, no need to check
            if (!modifiedAttributeCache.TryToModified(PrimaryAttribute, out _)) return;
            
            // Assign the clamped updated attribute value directly
            AttributeValue primaryAttributeValue = attributeCache[PrimaryAttribute].Value;
            if (primaryAttributeValue.BaseValue < primaryAttributeValue.CurrentValue)
            {
                // modifiedAttributeValue.DeltaCurrentValue -= primaryAttributeValue.CurrentValue - primaryAttributeValue.BaseValue;
                primaryAttributeValue.CurrentValue = primaryAttributeValue.BaseValue;
            }
            else return;

            Debug.Log("$CLAMPING");

            attributeCache[PrimaryAttribute].Clamp(EAttributeModificationMethod.FromLast, default, primaryAttributeValue);
            
            // Set the modified attribute value to reflect the actual modification post clamp
            // modifiedAttributeCache.Set(PrimaryAttribute, modifiedAttributeValue);
        }
    }
}
