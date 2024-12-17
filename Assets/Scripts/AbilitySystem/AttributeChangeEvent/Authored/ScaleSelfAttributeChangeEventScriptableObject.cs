using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    /// <summary>
    /// Scales an attribute's current value based on modifications to its base value
    /// </summary>
    [CreateAssetMenu(menuName = "FESGAS/Authored/Attribute Change Event/Scale Self", fileName = "New Scale Self Event")]
    public class ScaleSelfAttributeChangeEventScriptableObject : AbstractAttributeChangeEventScriptableObject
    {
        public override void PreAttributeChange(AttributeSystemComponent abilitySystem, ref Dictionary<AttributeScriptableObject, AttributeValue> attributeCache, ref Dictionary<AttributeScriptableObject, ModifiedAttributeValue> modifiedAttributeCache)
        {
            // Scale self shouldn't implement anything here
            if (!modifiedAttributeCache.TryGetValue(PrimaryAttribute, out ModifiedAttributeValue modifiedAttributeValue)) return;
            if (modifiedAttributeValue.DeltaBaseValue == 0f) return;

            float proportion = attributeCache[PrimaryAttribute].CurrentValue / attributeCache[PrimaryAttribute].BaseValue;
            modifiedAttributeValue.DeltaCurrentValue = proportion * modifiedAttributeValue.DeltaBaseValue;
            modifiedAttributeCache[PrimaryAttribute] = modifiedAttributeValue;
        }
        public override void PostAttributeChange(AttributeSystemComponent abilitySystem, ref Dictionary<AttributeScriptableObject, AttributeValue> attributeCache, ref Dictionary<AttributeScriptableObject, ModifiedAttributeValue> modifiedAttributeCache)
        {
            /*if (!modifiedAttributeCache.TryGetValue(PrimaryAttribute, out ModifiedAttributeValue _)) return;

            float proportion = attributeCache[PrimaryAttribute].CurrentValue / attributeCache[PrimaryAttribute].BaseValue;
            attributeCache[PrimaryAttribute] = new AttributeValue(
                attributeCache[PrimaryAttribute].CurrentValue * proportion,
                attributeCache[PrimaryAttribute].BaseValue
            );*/
        }
    }
}
