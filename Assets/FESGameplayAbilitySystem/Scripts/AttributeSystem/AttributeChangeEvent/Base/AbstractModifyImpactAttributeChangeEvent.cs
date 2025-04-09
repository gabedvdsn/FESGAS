using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace FESGameplayAbilitySystem
{
    public abstract class AbstractModifyImpactAttributeChangeEvent : MathAttributeChangeEventScriptableObject
    {
        [Header("Modify Impact Event")]
        
        public EImpactType ModifyType;
        public EEffectImpactTargetExpanded ModifyTarget;
        [Tooltip("Validate that exclusively the modify target is modified, as opposed to itself AND the alternative (e.g. target is Current when Current AND Base are modified would NOT pass validation.")]
        public bool ModifyTargetExclusive = false;
        
        public override bool ValidateWorkFor(GASComponentBase system, ref Dictionary<AttributeScriptableObject, CachedAttributeValue> attributeCache, SourcedModifiedAttributeCache modifiedAttributeCache)
        {
            if (!base.ValidateWorkFor(system, ref attributeCache, modifiedAttributeCache)) return false;

            if (!modifiedAttributeCache.TryToModified(TargetAttribute, out var delta)) return false;
            if (!GASHelper.ValidateImpactTargets(ModifyTarget, delta.ToAttributeValue(), ModifyTargetExclusive)) return false;

            return true;
        }
    }
}
    