using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace FESGameplayAbilitySystem
{
    /// <summary>
    /// Multiplies the SMAVs under the primary attribute by the current value
    /// </summary>
    [CreateAssetMenu(menuName = "FESGAS/Attribute/Change Event/Multiply Relative", fileName = "ACE_MultiplyRelative_")]
    public class MathAttributeChangeEventScriptableObject : AbstractRelativeAttributeChangeEventScriptableObject
    {
        [Header("Math Event")]
        
        public ESignPolicy SignPolicy;
        public ECalculationOperation Operation = ECalculationOperation.Multiply;
        public bool AllowSelfModification;
        
        public override void AttributeChangeEvent(GASComponentBase system, ref Dictionary<AttributeScriptableObject, CachedAttributeValue> attributeCache,
            SourcedModifiedAttributeCache modifiedAttributeCache)
        {
            if (!modifiedAttributeCache.AttributeIsActive(TargetAttribute)) return;
            switch (Operation)
            {
                case ECalculationOperation.Add:
                    modifiedAttributeCache.Add(TargetAttribute, SignPolicy, attributeCache[RelativeTo].Value * RelativeMultiplier, AllowSelfModification, ApplyAbilityContextTags, AnyContextTag);
                    break;
                case ECalculationOperation.Multiply:
                    modifiedAttributeCache.Multiply(TargetAttribute, SignPolicy, attributeCache[RelativeTo].Value * RelativeMultiplier, AllowSelfModification, ApplyAbilityContextTags, AnyContextTag);
                    break;
                case ECalculationOperation.Override:
                    modifiedAttributeCache.Override(TargetAttribute, SignPolicy, attributeCache[RelativeTo].Value * RelativeMultiplier, AllowSelfModification, ApplyAbilityContextTags, AnyContextTag);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override int GetPriority()
        {
            return Operation switch
            {

                ECalculationOperation.Add => 0,
                ECalculationOperation.Multiply => 1,
                ECalculationOperation.Override => 2,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    public enum ESignPolicy
    {
        Negative,
        Positive,
        ZeroBiased,
        ZeroNeutral
    }
}
