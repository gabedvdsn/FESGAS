using System;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(fileName = "AIW_", menuName = "FESGAS/Ability/Impact Worker/Relative Operation Context Worker", order = 0)]
    public class RelativeOperationContextImpactWorkerScriptableObject : AbstractContextImpactWorkerScriptableObject
    {
        [Header("Relative & Operation")]
        
        [Tooltip("The attribute to modify relative to the primary attribute and operation")]
        public AttributeScriptableObject RelativeAttribute;
        public ECalculationOperation Operation;
        
        [Header("Danger! [ KEEP FALSE ]")]
        [Tooltip("Only set TRUE if you know what you are doing. Keeping this FALSE will ensure cycles are broken without recursing once.\nExample: Receiving health via magic triggers worker to provide health via magic, resulting in endless cycle.")]
        public bool WorkerImpactWorkable = false;
        
        protected override void PerformImpactResponse(AbilityImpactData impactData)
        {
            // Extract the relative attribute (e.g. lifesteal attribute)
            if (!impactData.SourcedModifier.Derivation.GetSource().AttributeSystem.TryGetAttributeValue(RelativeAttribute, out AttributeValue relValue)) return;
            AttributeValue attributeValue;

            switch (Operation)
            {
                case ECalculationOperation.Add:
                    attributeValue = relValue + impactData.RealImpact;
                    break;
                case ECalculationOperation.Multiply:
                    attributeValue = relValue * impactData.RealImpact;
                    break;
                case ECalculationOperation.Override:
                    attributeValue = relValue;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            attributeValue = GASHelper.AlignToSign(attributeValue, WorkSignPolicy);

            SourcedModifiedAttributeValue sourcedModifier = new SourcedModifiedAttributeValue(
                IAttributeImpactDerivation.GenerateSourceDerivation(impactData.SourcedModifier, WorkImpactType),
                attributeValue.CurrentValue, attributeValue.BaseValue,
                WorkerImpactWorkable
            );
            if (WorkSameFrame) impactData.SourcedModifier.Derivation.GetSource().AttributeSystem.ModifyAttributeImmediate(WorkAttribute, sourcedModifier);
            else impactData.SourcedModifier.Derivation.GetSource().AttributeSystem.ModifyAttribute(WorkAttribute, sourcedModifier);
        }
    }
}
