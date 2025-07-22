using System;
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
        public override void AttributeChangeEvent(GASComponentBase system, Dictionary<AttributeScriptableObject, CachedAttributeValue> attributeCache,
            ChangeValue change)
        {
            float proportion = change.Value.BaseValue / attributeCache[TargetAttribute].Value.BaseValue;
            float delta = proportion * attributeCache[TargetAttribute].Value.CurrentValue;
            
            IAttributeImpactDerivation scaleDerivation = IAttributeImpactDerivation.GenerateSourceDerivation(change.Value, EImpactType.NotApplicable, false);
            SourcedModifiedAttributeValue scaleAmount = new SourcedModifiedAttributeValue(scaleDerivation, delta, 0f, false);
            if (system.FindAttributeSystem(out var attr)) attr.ModifyAttribute(TargetAttribute, scaleAmount, runEvents: false);
        }

        public override bool ValidateWorkFor(GASComponentBase system, Dictionary<AttributeScriptableObject, CachedAttributeValue> attributeCache, ChangeValue change)
        {
            return change.Value.BaseValue != 0 && base.ValidateWorkFor(system, attributeCache, change);
        }
    }
}
