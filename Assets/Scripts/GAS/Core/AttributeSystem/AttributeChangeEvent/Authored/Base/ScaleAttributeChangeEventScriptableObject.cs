using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    /// <summary>
    /// Scales an attribute's current value based on modifications to its base value
    /// </summary>
    [CreateAssetMenu(menuName = "FESGAS/Authored/Attribute Change Event/Scale", fileName = "ACE_Scale")]
    public class ScaleAttributeChangeEventScriptableObject : AbstractFocusedAttributeChangeEventScriptableObject
    {
        public override void PreAttributeChange(GASComponent system, ref Dictionary<AttributeScriptableObject, CachedAttributeValue> attributeCache, SourcedModifiedAttributeCache modifiedAttributeCache)
        {
            if (!modifiedAttributeCache.TryToModified(TargetAttribute, out ModifiedAttributeValue modifiedAttributeValue)) return;
            if (modifiedAttributeValue.DeltaBaseValue == 0f) return;

            float proportion = attributeCache[TargetAttribute].Value.CurrentValue / attributeCache[TargetAttribute].Value.BaseValue;
            modifiedAttributeValue.DeltaCurrentValue = proportion * modifiedAttributeValue.DeltaBaseValue;

            modifiedAttributeCache.Override(TargetAttribute, modifiedAttributeValue);
        }
        
        public override void PostAttributeChange(GASComponent system, ref Dictionary<AttributeScriptableObject, CachedAttributeValue> attributeCache, SourcedModifiedAttributeCache modifiedAttributeCache)
        {
            // Scale self shouldn't implement anything here
        }
    }
}
