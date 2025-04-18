﻿using System.Collections.Generic;
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

            // Proportion of base value to old base value
            float pBase = attributeCache[TargetAttribute].Value.BaseValue / (attributeCache[TargetAttribute].Value.BaseValue - modifiedAttributeValue.DeltaBaseValue);
            // Current value delta
            float deltaCurr = attributeCache[TargetAttribute].Value.CurrentValue * pBase + (pBase > 1 ? -attributeCache[TargetAttribute].Value.CurrentValue : 0);

            Debug.Log($"delta curr: {deltaCurr} ({pBase})");
            
            //attributeCache[TargetAttribute].Add(IAttributeImpactDerivation.GenerateSourceDerivation(system, TargetAttribute, retainImpact: false), new AttributeValue(deltaCurr, 0));
            float proportion = attributeCache[TargetAttribute].Value.CurrentValue / attributeCache[TargetAttribute].Value.BaseValue;
            modifiedAttributeValue.DeltaCurrentValue = proportion * modifiedAttributeValue.DeltaBaseValue;
            // modifiedAttributeValue.DeltaCurrentValue = (1 + proportion) * attributeCache[TargetAttribute].Value.CurrentValue;
            
            modifiedAttributeCache.Override(TargetAttribute, modifiedAttributeValue.ToAttributeValue(), true, null, true);
        }
    }
}
