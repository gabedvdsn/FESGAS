using System;
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
        [Header("Clamp Attribute Change Event")]
        
        public EClampEventPolicy ClampPolicy;
        
        [Space]
        
        public AttributeValue SetCeil;
        public AttributeValue SetFloor;
        
        public override void PreAttributeChange(GASComponent system, ref Dictionary<AttributeScriptableObject, CachedAttributeValue> attributeCache, SourcedModifiedAttributeCache modifiedAttributeCache)
        {
            // Clamp shouldn't implement anything here
        }
        
        public override void PostAttributeChange(GASComponent system, ref Dictionary<AttributeScriptableObject, CachedAttributeValue> attributeCache, SourcedModifiedAttributeCache modifiedAttributeCache)
        {
            // If the primary attribute was never modified, no need to check
            if (!modifiedAttributeCache.TryToModified(TargetAttribute, out _)) return;
            
            AttributeValue clampValue = attributeCache[TargetAttribute].Value;
            AttributeValue baseAligned = clampValue.BaseAligned();

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
            
            /*switch (ClampPolicy)
            {

                case EClampEventPolicy.CeilBaseFloorZero:
                    if (AttributeValue.WithinLimits(clampValue, default, baseAligned)) return;
                    attributeCache[PrimaryAttribute].Clamp(default, baseAligned);
                    break;
                case EClampEventPolicy.CeilBaseFloorSet:
                    if (AttributeValue.WithinLimits(clampValue, SetFloor, baseAligned)) return;
                    attributeCache[PrimaryAttribute].Clamp(SetFloor, baseAligned);
                    break;
                case EClampEventPolicy.CeilSetFloorZero:
                    if (AttributeValue.WithinLimits(clampValue, default, SetCeil)) return;
                    attributeCache[PrimaryAttribute].Clamp(default, SetCeil);
                    break;
                case EClampEventPolicy.AllSet:
                    if (AttributeValue.WithinLimits(clampValue, SetFloor, SetCeil)) return;
                    attributeCache[PrimaryAttribute].Clamp(SetFloor, SetCeil);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }*/
            
            // Assign the clamped updated attribute value directly
            
            /*
            if (clampValue.BaseValue < clampValue.CurrentValue)
            {
                clampValue.CurrentValue = clampValue.BaseValue;
            }
            else return;
            
            attributeCache[PrimaryAttribute].Clamp(default, clampValue);*/
        }
    }

    public enum EClampEventPolicy
    {
        CeilBaseFloorZero,
        CeilBaseFloorSet,
        CeilSetFloorZero,
        AllSet
    }
}
