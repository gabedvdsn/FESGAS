using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace FESGameplayAbilitySystem
{
    /// <summary>
    /// Multiplies the SMAVs under the primary attribute by the current value
    /// </summary>
    [CreateAssetMenu(menuName = "FESGAS/Attribute/Change Event/Logical Math", fileName = "ACE_LogicalMath_")]
    public class MathAttributeChangeEventScriptableObject : AbstractRelativeAttributeChangeEventScriptableObject
    {
        [Header("Math Event")]
        
        public ECalculationOperation Operation = ECalculationOperation.Multiply;
        public EEffectImpactTarget OperationTarget;
        public EMathApplicationPolicy OperationPolicy;
        
        public override void AttributeChangeEvent(GASComponentBase system, Dictionary<IAttribute, CachedAttributeValue> attributeCache,
            ChangeValue change)
        {
            var result = GASHelper.AttributeMathEvent(change.Value.ToAttributeValue(), GetRelative(attributeCache, change), Operation, OperationTarget, OperationPolicy);
            change.Override(result);
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

    public enum ESignPolicyExtended
    {
        Negative,
        Positive,
        ZeroBiased,
        ZeroNeutral,
        Any
    }

    public enum EMathApplicationPolicy
    {
        AsIs,
        OnePlus,
        OneMinus
    }
}
