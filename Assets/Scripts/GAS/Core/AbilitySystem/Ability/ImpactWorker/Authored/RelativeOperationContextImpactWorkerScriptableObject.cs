using System;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(fileName = "AEW_RelOpCntx", menuName = "FESGAS/Ability/Worker/Relative Operation Context Worker", order = 0)]
    public class RelativeOperationContextImpactWorkerScriptableObject : AbstractContextImpactWorkerScriptableObject
    {
        [Header("Relative & Operation")]
        
        [Tooltip("The attribute to modify relative to the primary attribute and operation")]
        public AttributeScriptableObject RelativeAttribute;
        public CalculationOperation Operation;
        
        [Header("Danger! [ KEEP FALSE ]")]
        [Tooltip("Only adjust if you know what you are doing. Keeping this FALSE will ensure cycles are broken without recursing once.")]
        public bool WorkerImpactWorkable = false;
        
        protected override void PerformImpactResponse(AbilityImpactData impactData)
        {
            // Extract the relative attribute (e.g. lifesteal attribute)
            if (!impactData.SourcedModifier.Derivation.GetSource().AttributeSystem.TryGetAttributeValue(RelativeAttribute, out AttributeValue relValue)) return;
            AttributeValue attributeValue;

            switch (Operation)
            {
                case CalculationOperation.Add:
                    attributeValue = relValue + impactData.RealImpact;
                    break;
                case CalculationOperation.Multiply:
                    attributeValue = relValue * impactData.RealImpact;
                    break;
                case CalculationOperation.Override:
                    attributeValue = relValue;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            attributeValue = GASHelper.AlignToSign(attributeValue, WorkSignPolicy);

            SourcedModifiedAttributeValue sourcedModifier = new SourcedModifiedAttributeValue(
                IAttributeDerivation.GenerateSourceDerivation(impactData.SourcedModifier, WorkImpactType),
                attributeValue.CurrentValue, attributeValue.BaseValue,
                WorkerImpactWorkable
            );
            if (WorkSameFrame) impactData.SourcedModifier.Derivation.GetSource().AttributeSystem.ModifyAttributeImmediate(WorkAttribute, sourcedModifier);
            else impactData.SourcedModifier.Derivation.GetSource().AttributeSystem.ModifyAttribute(WorkAttribute, sourcedModifier);
        }
    }
}
