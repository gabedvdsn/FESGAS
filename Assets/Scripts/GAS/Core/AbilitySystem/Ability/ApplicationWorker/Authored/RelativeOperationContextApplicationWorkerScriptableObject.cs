using System;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(fileName = "AAW_Group", menuName = "FESGAS/Ability/Application Worker/Relative Operation Context Worker", order = 0)]
    public class RelativeOperationContextApplicationWorkerScriptableObject : AbstractContextApplicationWorkerScriptableObject
    {
        [Header("Relative & Operation")] 
        
        public AttributeScriptableObject RelativeAttribute;
        public ECalculationOperation Operation;
        [Tooltip("Is the relative attribute a modifier? E.g. damage amplification")]
        public bool IsModifier = true;
        
        public override SourcedModifiedAttributeValue ModifyImpact(GASComponent target, SourcedModifiedAttributeValue smav)
        {
            if (!smav.BaseDerivation.GetSource().AttributeSystem.TryGetAttributeValue(RelativeAttribute, out AttributeValue _relValue)) return smav;

            Debug.Log($"\t{name} {smav.BaseDerivation.GetSource()} {target}");

            AttributeValue relValue = IsModifier ? new AttributeValue(1 + _relValue.CurrentValue, 1 + _relValue.BaseValue) : _relValue;
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

        private AttributeValue ComputeModifiedImpact(AttributeValue relValue, AttributeValue impactValue)
        {
            return Operation switch
            {
                ECalculationOperation.Add => impactValue + relValue,
                ECalculationOperation.Multiply => impactValue * relValue,
                ECalculationOperation.Override => relValue,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
        public override bool ValidateWorkFor(GASComponent target, SourcedModifiedAttributeValue smav)
        {
            return base.ValidateWorkFor(target, smav) && smav.BaseDerivation.GetSource().AttributeSystem.DefinesAttribute(RelativeAttribute);
        }
    }
}
