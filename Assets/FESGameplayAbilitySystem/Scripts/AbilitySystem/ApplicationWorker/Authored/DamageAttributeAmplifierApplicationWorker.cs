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

            Debug.Log($"\t{name} {smav.BaseDerivation.GetSource()} {target}");

            AttributeValue relValue = new AttributeValue(1 + _relValue.CurrentValue, 1 + _relValue.BaseValue);
            AttributeValue impactValue = ComputeModifiedImpact(relValue, new AttributeValue(smav.DeltaCurrentValue, smav.DeltaBaseValue));
            
            return ApplicationTarget switch
            {
                EEffectImpactTarget.Current => new SourcedModifiedAttributeValue(smav.Derivation, smav.BaseDerivation, impactValue.CurrentValue, smav.DeltaBaseValue, smav.Workable),
                EEffectImpactTarget.Base => new SourcedModifiedAttributeValue(smav.Derivation, smav.BaseDerivation, smav.DeltaCurrentValue, impactValue.BaseValue, smav.Workable),
                EEffectImpactTarget.CurrentAndBase => new SourcedModifiedAttributeValue(smav.Derivation, smav.BaseDerivation, impactValue.CurrentValue, impactValue.BaseValue,
                    smav.Workable),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        public override bool ValidateWorkFor(GASComponentBase target, SourcedModifiedAttributeValue smav)
        {
            return base.ValidateWorkFor(target, smav);
        }
    }
}
