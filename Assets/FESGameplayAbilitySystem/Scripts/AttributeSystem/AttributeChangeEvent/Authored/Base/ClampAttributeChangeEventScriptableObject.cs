﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    /// <summary>
    /// Clamps an attributes current value based on its base value
    /// </summary>
    [CreateAssetMenu(menuName = "FESGAS/Attribute/Change Event/Clamp", fileName = "ACE_Clamp")]
    public class ClampAttributeChangeEventScriptableObject : AbstractFocusedAttributeChangeEventScriptableObject
    {
        public override void AttributeChangeEvent(GASComponentBase system, ref Dictionary<AttributeScriptableObject, CachedAttributeValue> attributeCache,
            SourcedModifiedAttributeCache modifiedAttributeCache)
        {
            AttributeValue clampValue = attributeCache[TargetAttribute].Value;
            AttributeValue baseAligned = clampValue.BaseAligned();

            // Clamp bounds logic is derived from the overflow policy associated with the target attribute
            switch (attributeCache[TargetAttribute].Overflow.Policy)
            {
                case EAttributeOverflowPolicy.ZeroToBase:
                    if (AttributeValue.WithinLimits(clampValue, default, baseAligned)) return;
                    attributeCache[TargetAttribute].Clamp(baseAligned);
                    break;
                case EAttributeOverflowPolicy.FloorToBase:
                    if (AttributeValue.WithinLimits(clampValue, attributeCache[TargetAttribute].Overflow.Floor, baseAligned)) return;
                    attributeCache[TargetAttribute].Clamp(attributeCache[TargetAttribute].Overflow.Floor, baseAligned);
                    break;
                case EAttributeOverflowPolicy.ZeroToCeil:
                    if (AttributeValue.WithinLimits(clampValue, default, attributeCache[TargetAttribute].Overflow.Ceil)) return;
                    attributeCache[TargetAttribute].Clamp(attributeCache[TargetAttribute].Overflow.Ceil);
                    break;
                case EAttributeOverflowPolicy.FloorToCeil:
                    if (AttributeValue.WithinLimits(clampValue, attributeCache[TargetAttribute].Overflow.Floor, attributeCache[TargetAttribute].Overflow.Ceil)) return;
                    attributeCache[TargetAttribute].Clamp(attributeCache[TargetAttribute].Overflow.Floor, attributeCache[TargetAttribute].Overflow.Ceil);
                    break;
                case EAttributeOverflowPolicy.Unlimited:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
