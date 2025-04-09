using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    /// <summary>
    /// Scales an attribute's current value based on modifications to its base value
    /// </summary>
    [CreateAssetMenu(menuName = "FESGAS/Attribute/Change Event/Scale", fileName = "ACE_Scale_")]
    public class ScaleAttributeChangeEventScriptableObject : AbstractFocusedAttributeChangeEventScriptableObject
    {
        public override void AttributeChangeEvent(GASComponentBase system, ref Dictionary<AttributeScriptableObject, CachedAttributeValue> attributeCache,
            SourcedModifiedAttributeCache modifiedAttributeCache)
        {
            if (!modifiedAttributeCache.TryToModified(TargetAttribute, out ModifiedAttributeValue modifiedAttributeValue)) return;
            if (modifiedAttributeValue.DeltaBaseValue == 0f) return;

            float proportion = attributeCache[TargetAttribute].Value.CurrentValue / attributeCache[TargetAttribute].Value.BaseValue;
            modifiedAttributeValue.DeltaCurrentValue = proportion * modifiedAttributeValue.DeltaBaseValue;

            modifiedAttributeCache.Override(TargetAttribute, modifiedAttributeValue.ToAttributeValue(), true, null, true);
        }
    }
}
