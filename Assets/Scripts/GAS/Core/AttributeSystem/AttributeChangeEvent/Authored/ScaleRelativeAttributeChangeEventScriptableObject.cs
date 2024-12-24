using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    /// <summary>
    /// Scales an attribute's current value based on modifications to its base value
    /// </summary>
    [CreateAssetMenu(menuName = "FESGAS/Authored/Attribute Change Event/Scale Relative", fileName = "ACE_ScaleRelative")]
    public class ScaleRelativeAttributeChangeEventScriptableObject : AbstractAttributeChangeEventScriptableObject
    {
        public override void PreAttributeChange(GASComponent system, ref Dictionary<AttributeScriptableObject, AttributeValue> attributeCache, ref Dictionary<AttributeScriptableObject, ModifiedAttributeValue> modifiedAttributeCache)
        {
            if (!modifiedAttributeCache.TryGetValue(PrimaryAttribute, out ModifiedAttributeValue modifiedAttributeValue)) return;
            if (modifiedAttributeValue.DeltaBaseValue == 0f) return;

            float proportion = attributeCache[PrimaryAttribute].CurrentValue / attributeCache[PrimaryAttribute].BaseValue;
            modifiedAttributeValue.DeltaCurrentValue = proportion * modifiedAttributeValue.DeltaBaseValue;
            modifiedAttributeCache[PrimaryAttribute] = modifiedAttributeValue;
        }
        
        public override void PostAttributeChange(GASComponent system, ref Dictionary<AttributeScriptableObject, AttributeValue> attributeCache, ref Dictionary<AttributeScriptableObject, ModifiedAttributeValue> modifiedAttributeCache)
        {
            // Scale self shouldn't implement anything here
        }
    }
}
