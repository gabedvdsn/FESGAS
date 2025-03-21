using System;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public static class GASHelper
    {
        public static ESignPolicy DeterminePolicy(params float[] magnitudes)
        {
            float sum = magnitudes.Sum();
            return sum switch
            {
                > 0 => ESignPolicy.Positive,
                < 0 => ESignPolicy.Negative,
                0 when magnitudes.Any(mag => mag != 0) => ESignPolicy.ZeroBiased,
                _ => ESignPolicy.ZeroNeutral
            };
        }
        
        public static int SignInt(ESignPolicy signPolicy)
        {
            return signPolicy switch
            {

                ESignPolicy.Negative => -1,
                ESignPolicy.Positive => 1,
                ESignPolicy.ZeroBiased => 0,
                ESignPolicy.ZeroNeutral => 0,
                _ => throw new ArgumentOutOfRangeException(nameof(signPolicy), signPolicy, null)
            };
        }
        
        public static float SignFloat(ESignPolicy signPolicy)
        {
            return signPolicy switch
            {

                ESignPolicy.Negative => -1f,
                ESignPolicy.Positive => 1f,
                ESignPolicy.ZeroBiased => 0f,
                ESignPolicy.ZeroNeutral => 0f,
                _ => throw new ArgumentOutOfRangeException(nameof(signPolicy), signPolicy, null)
            };
        }

        public static int AlignedSignInt(params ESignPolicy[] signPolicies)
        {
            return signPolicies.Aggregate(1, (current, signPolicy) => current * SignInt(signPolicy));
        }
        
        public static float AlignedSignFloat(params ESignPolicy[] signPolicies)
        {
            return signPolicies.Aggregate(1f, (current, signPolicy) => current * SignFloat(signPolicy));
        }

        public static int AlignToSign(int value, ESignPolicy signPolicy)
        {
            int _value = Mathf.Abs(value);
            return signPolicy switch
            {

                ESignPolicy.Negative => -_value,
                ESignPolicy.Positive => _value,
                ESignPolicy.ZeroBiased => _value,
                ESignPolicy.ZeroNeutral => _value,
                _ => throw new ArgumentOutOfRangeException(nameof(signPolicy), signPolicy, null)
            };
        }
        
        public static float AlignToSign(float value, ESignPolicy signPolicy)
        {
            float _value = Mathf.Abs(value);
            return signPolicy switch
            {

                ESignPolicy.Negative => -_value,
                ESignPolicy.Positive => _value,
                ESignPolicy.ZeroBiased => _value,
                ESignPolicy.ZeroNeutral => _value,
                _ => throw new ArgumentOutOfRangeException(nameof(signPolicy), signPolicy, null)
            };
        }
        
        public static AttributeValue AlignToSign(AttributeValue attributeValue, ESignPolicy signPolicy)
        {
            float _curr = Mathf.Abs(attributeValue.CurrentValue);
            float _base= Mathf.Abs(attributeValue.BaseValue);

            switch (signPolicy)
            {
                case ESignPolicy.Negative:
                    return new AttributeValue(-_curr, -_base);
                case ESignPolicy.Positive:
                case ESignPolicy.ZeroBiased:
                case ESignPolicy.ZeroNeutral:
                    return new AttributeValue(_curr, _base);
                default:
                    throw new ArgumentOutOfRangeException(nameof(signPolicy), signPolicy, null);
            }
        }

        public static EImpactType FromImpactTypeAny(EImpactTypeAny impactType)
        {
            return impactType switch
            {

                EImpactTypeAny.NotApplicable => EImpactType.NotApplicable,
                EImpactTypeAny.Physical => EImpactType.Physical,
                EImpactTypeAny.Magical => EImpactType.Magical,
                EImpactTypeAny.Pure => EImpactType.Pure,
                EImpactTypeAny.Any => EImpactType.NotApplicable,
                _ => throw new ArgumentOutOfRangeException(nameof(impactType), impactType, null)
            };
        }

        public static bool ValidateImpactTypes(EImpactType impactType, EImpactTypeAny impactTypeAny)
        {
            return impactTypeAny switch
            {

                EImpactTypeAny.NotApplicable => impactType == EImpactType.NotApplicable,
                EImpactTypeAny.Physical => impactType == EImpactType.Physical,
                EImpactTypeAny.Magical => impactType == EImpactType.Magical,
                EImpactTypeAny.Pure => impactType == EImpactType.Pure,
                EImpactTypeAny.Any => true,
                _ => throw new ArgumentOutOfRangeException(nameof(impactTypeAny), impactTypeAny, null)
            };
        }
    }
}
