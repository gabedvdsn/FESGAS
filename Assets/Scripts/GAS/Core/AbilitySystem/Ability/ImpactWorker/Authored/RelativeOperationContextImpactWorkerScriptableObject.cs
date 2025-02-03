using System;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public class RelativeOperationContextImpactWorkerScriptableObject : AbstractContextImpactWorkerScriptableObject
    {
        [Tooltip("The attribute to modify relative to the primary attribute and operation")]
        public AttributeScriptableObject RelativeAttribute;
        public CalculationOperation Operation;
        protected override void PerformImpactResponse(AbilityImpactData impactData)
        {
            if (!impactData.SourcedModifier.Derivation.GetSource().AttributeSystem.TryGetAttributeValue(PrimaryAttribute, out CachedAttributeValue primaryAttribute)) return;
            AttributeValue attributeValue;

            switch (Operation)
            {

                case CalculationOperation.Add:
                    attributeValue = primaryAttribute.Value + impactData.RealImpact;
                    break;
                case CalculationOperation.Multiply:
                    attributeValue = primaryAttribute.Value * impactData.RealImpact;
                    break;
                case CalculationOperation.Override:
                    attributeValue = primaryAttribute.Value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            SourcedModifiedAttributeValue sourcedModifier = new SourcedModifiedAttributeValue(
                IAttributeDerivation.GenerateSourceDerivation(impactData.SourcedModifier),
                attributeValue.CurrentValue, attributeValue.BaseValue
            );
            if (ApplySameFrame) impactData.SourcedModifier.Derivation.GetEffectDerivation().GetOwner().AttributeSystem.ModifyAttributeImmediate(RelativeAttribute, sourcedModifier);
        }
    }
}
