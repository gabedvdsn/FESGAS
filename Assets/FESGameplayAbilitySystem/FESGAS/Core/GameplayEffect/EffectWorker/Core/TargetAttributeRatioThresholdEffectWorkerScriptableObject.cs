using System;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public abstract class TargetAttributeRatioThresholdEffectWorkerScriptableObject : TargetAttributeThresholdEffectWorkerScriptableObject
    {
        protected override bool MeetsThreshold(AttributeValue attributeValue, Func<float, float, bool> policyFunc)
        {
            return Target switch
            {

                EEffectImpactTarget.Current => policyFunc(attributeValue.CurrentValue / attributeValue.BaseValue, Threshold.CurrentValue),
                EEffectImpactTarget.Base => policyFunc(attributeValue.BaseValue / attributeValue.BaseValue, Threshold.CurrentValue),
                EEffectImpactTarget.CurrentAndBase => policyFunc(attributeValue.CurrentValue / attributeValue.BaseValue, Threshold.CurrentValue) &&
                                                      policyFunc(attributeValue.BaseValue / attributeValue.BaseValue, Threshold.CurrentValue),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
        
    }
}
