using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public static class Gasify
    {
        public static class Modifier
        {
            #region Simple

            public static void Init_Simple(AnimationCurve scaling)
            {
                
            }
            
            public static float Eval_Simple(AnimationCurve scaling, IAttributeImpactDerivation spec)
            {
                return scaling.Evaluate(spec.GetEffectDerivation().GetRelativeLevel());
            }
            
            #endregion
            
            #region Group
            
            public static void Init_Group(List<MagnitudeModifierGroupScriptableObject.MagnitudeModifierGroupMember> members, IAttributeImpactDerivation spec)
            {
                foreach (var member in members) member.Calculation.Initialize(spec);
            }

            public static float Eval_Group(List<MagnitudeModifierGroupScriptableObject.MagnitudeModifierGroupMember> members, EValueCollisionPolicy collisionPolicy, IAttributeImpactDerivation spec)
            {
                if (members.Any(m => m.RelativeOperation == ECalculationOperation.Override))
                {
                    return collisionPolicy switch
                    {
                        EValueCollisionPolicy.UseMaximum => members.Where(m => m.RelativeOperation == ECalculationOperation.Override)
                            .Max(m => m.Calculation.Evaluate(spec)),
                        EValueCollisionPolicy.UseMinimum => members.Where(m => m.RelativeOperation == ECalculationOperation.Override)
                            .Min(m => m.Calculation.Evaluate(spec)),
                        EValueCollisionPolicy.UseAverage => members.Where(m => m.RelativeOperation == ECalculationOperation.Override)
                            .Average(m => m.Calculation.Evaluate(spec)),
                        _ => throw new ArgumentOutOfRangeException()
                    };
                }
                
                float value = members.Where(m => m.RelativeOperation == ECalculationOperation.Add).Sum(member => member.Calculation.Evaluate(spec));
                return members.Where(m => m.RelativeOperation == ECalculationOperation.Multiply).Aggregate(value, (current, member) => current * member.Calculation.Evaluate(spec));
            }
            
            #endregion
            
            #region Attribute Backed

            public static void Init_AttributeBacked(IMagnitudeModifier modifier, IAttribute capture, ECaptureAttributeWhen captureWhen, ESourceTarget captureFrom, IAttributeImpactDerivation spec)
            {
                if (captureWhen != ECaptureAttributeWhen.OnCreation) return;
                
                switch (captureFrom)
                {
                    case ESourceTarget.Source:
                        if (!spec.GetSource().FindAttributeSystem(out var attr) || !attr.TryGetAttributeValue(capture, out AttributeValue sourceAttributeValue)) break;
                        spec.GetSourcedCapturedAttributes()[modifier] = sourceAttributeValue;
                        break;
                    case ESourceTarget.Target:
                        if (!spec.GetTarget().FindAttributeSystem(out var attr2) || !attr2.TryGetAttributeValue(capture, out AttributeValue targetAttributeValue)) break;
                        spec.GetSourcedCapturedAttributes()[modifier] = targetAttributeValue;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            public static float Eval_AttributeBacked(IMagnitudeModifier modifier, AnimationCurve scaling, EEffectImpactTargetLimited scalingPolicy, IAttribute capture, ECaptureAttributeWhen captureWhen, ESourceTarget captureFrom, IAttributeImpactDerivation spec)
            {
                if (captureWhen == ECaptureAttributeWhen.OnCreation)
                {
                    return scalingPolicy switch
                    {
                        EEffectImpactTargetLimited.Current => scaling.Evaluate(spec.GetSourcedCapturedAttributes()[modifier].GetValueOrDefault().CurrentValue),
                        EEffectImpactTargetLimited.Base => scaling.Evaluate(spec.GetSourcedCapturedAttributes()[modifier].GetValueOrDefault().BaseValue),
                        _ => throw new ArgumentOutOfRangeException()
                    };
                }

                if (captureFrom == ESourceTarget.Source)
                {
                    if (!spec.GetSource().FindAttributeSystem(out var attr) || !attr.TryGetAttributeValue(capture, out AttributeValue attributeValue)) return 0f;
                    return scalingPolicy switch
                    {

                        EEffectImpactTargetLimited.Current => scaling.Evaluate(attributeValue.CurrentValue),
                        EEffectImpactTargetLimited.Base => scaling.Evaluate(attributeValue.BaseValue),
                        _ => throw new ArgumentOutOfRangeException()
                    };
                }
                else
                {
                    if (!spec.GetTarget().FindAttributeSystem(out var attr) || !attr.TryGetAttributeValue(capture, out AttributeValue attributeValue)) return 0f;
                    return scalingPolicy switch
                    {

                        EEffectImpactTargetLimited.Current => scaling.Evaluate(attributeValue.CurrentValue),
                        EEffectImpactTargetLimited.Base => scaling.Evaluate(attributeValue.BaseValue),
                        _ => throw new ArgumentOutOfRangeException()
                    };
                }
            }

            #endregion
        }
        
        
        
    }
}
