using System;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public abstract class TargetAttributeThresholdEffectWorkerScriptableObject : AbstractEffectWorkerScriptableObject
    {
        [Header("Focused Effect Worker")]
        
        public AttributeScriptableObject TargetAttribute;
        public AttributeValue Threshold;
        public EEffectImpactTarget Target;
        public EOverUnderEqual Policy;
        
        public override void OnEffectApplication(IAttributeImpactDerivation derivation)
        {
            // PerformThresholdWork(derivation);
        }
        public override void OnEffectRemoval(IAttributeImpactDerivation derivation)
        {
            // PerformThresholdWork(derivation);
        }
        public override void OnEffectImpact(AbilityImpactData impactData)
        {
            PerformThresholdWork(impactData.SourcedModifier.BaseDerivation);
        }

        protected void PerformThresholdWork(IAttributeImpactDerivation derivation)
        {
            if (!derivation.GetTarget().AttributeSystem.TryGetAttributeValue(TargetAttribute, out AttributeValue value)) return;
            switch (Policy)
            {

                case EOverUnderEqual.Over:
                    if (!MeetsThreshold(value, (v, a) => v > a)) return;
                    break;
                case EOverUnderEqual.Under:
                    if (!MeetsThreshold(value, (v, a) => v < a)) return;
                    break;
                case EOverUnderEqual.OverOrEqual:
                    if (!MeetsThreshold(value, (v, a) => v >= a)) return;
                    break;
                case EOverUnderEqual.UnderOrEqual:
                    if (!MeetsThreshold(value, (v, a) => v <= a)) return;
                    break;
                case EOverUnderEqual.Equal:
                    if (!MeetsThreshold(value, Mathf.Approximately)) return;
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            OnThresholdMet(derivation);
        }

        protected virtual bool MeetsThreshold(AttributeValue attributeValue, Func<float, float, bool> policyFunc)
        {
            return Target switch
            {
                EEffectImpactTarget.Current => policyFunc(attributeValue.CurrentValue, Threshold.CurrentValue),
                EEffectImpactTarget.Base => policyFunc(attributeValue.BaseValue, Threshold.BaseValue),
                EEffectImpactTarget.CurrentAndBase => policyFunc(attributeValue.CurrentValue, Threshold.CurrentValue) && policyFunc(attributeValue.BaseValue, Threshold.BaseValue),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        protected abstract void OnThresholdMet(IAttributeImpactDerivation derivation);
    }

    public enum EOverUnderEqual
    {
        Over,
        Under,
        OverOrEqual,
        UnderOrEqual,
        Equal
    }
}
