using System;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(fileName = "AAW_DamageAmp_", menuName = "FESGAS/Ability/Application Worker/Damage Amplification")]
    public class DamageAttributeAmplifierApplicationWorker : RelativeOperationContextApplicationWorkerScriptableObject
    {
        public override SourcedModifiedAttributeValue ModifyImpact(GASComponentBase target, SourcedModifiedAttributeValue smav)
        {
            if (!smav.BaseDerivation.GetSource().AttributeSystem.TryGetAttributeValue(RelativeAttribute, out AttributeValue _relValue)) return smav;
            
            AttributeValue relValue = new AttributeValue(1 + _relValue.CurrentValue, 1 + _relValue.BaseValue);
            AttributeValue impactValue = ComputeModifiedImpact(relValue, new AttributeValue(smav.CurrentValue, smav.BaseValue));
            
            return ApplicationTarget switch
            {
                EEffectImpactTargetExpanded.Current => new SourcedModifiedAttributeValue(smav.Derivation, smav.BaseDerivation, impactValue.CurrentValue, smav.BaseValue, smav.Workable),
                EEffectImpactTargetExpanded.Base => new SourcedModifiedAttributeValue(smav.Derivation, smav.BaseDerivation, smav.CurrentValue, impactValue.BaseValue, smav.Workable),
                EEffectImpactTargetExpanded.CurrentAndBase => new SourcedModifiedAttributeValue(smav.Derivation, smav.BaseDerivation, impactValue.CurrentValue, impactValue.BaseValue,
                    smav.Workable),
                EEffectImpactTargetExpanded.CurrentOrBase => GASHelper.ValidateImpactTargets(EEffectImpactTargetExpanded.Current, impactValue, ApplicationTargetExclusive) ? new SourcedModifiedAttributeValue(smav.Derivation, smav.BaseDerivation, impactValue.CurrentValue, smav.BaseValue, smav.Workable) : 
                    GASHelper.ValidateImpactTargets(EEffectImpactTargetExpanded.Base, impactValue, ApplicationTargetExclusive) ? new SourcedModifiedAttributeValue(smav.Derivation, smav.BaseDerivation, smav.CurrentValue, impactValue.BaseValue, smav.Workable) : new SourcedModifiedAttributeValue(smav.Derivation, smav.BaseDerivation, impactValue.CurrentValue, impactValue.BaseValue),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
