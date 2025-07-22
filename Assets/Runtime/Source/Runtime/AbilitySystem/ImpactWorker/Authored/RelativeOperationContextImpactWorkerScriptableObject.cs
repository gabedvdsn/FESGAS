﻿using System;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(fileName = "AIW_", menuName = "FESGAS/Ability/Impact Worker/Relative Operation Context Worker", order = 0)]
    public class RelativeOperationContextImpactWorkerScriptableObject : AbstractContextImpactWorkerScriptableObject
    {
        [Header("Relative & Operation")]
        
        [Tooltip("The attribute to use relative to the impact data (with respect to the primary attribute)")]
        public AttributeScriptableObject RelativeAttribute;
        public ECalculationOperation Operation;
        public ESourceTarget WithRespectTo;
        
        [Header("Danger! [ KEEP FALSE ]")]
        [Tooltip("Only set TRUE if you know what you are doing. Keeping this FALSE will ensure cycles are broken without recursing once.\nFor Example:\nA) Player receives health via magic\nB) Worker provides Player health via magic\nC) Cycle repeats infinitely")]
        public bool WorkerImpactWorkable = false;
        
        protected override void PerformImpactResponse(AbilityImpactData impactData)
        {
            // Extract the relative attribute (e.g. lifesteal attribute)
            if (!impactData.SourcedModifier.Derivation.GetSource().FindAttributeSystem(out var attr) || !attr.TryGetAttributeValue(RelativeAttribute, out AttributeValue relValue)) return;
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
            attr.ModifyAttribute(WorkAttribute, sourcedModifier);
        }

        public override bool ValidateWorkFor(AbilityImpactData impactData)
        {
            if (!base.ValidateWorkFor(impactData)) return false;
            return WithRespectTo switch
            {
                ESourceTarget.Target => impactData.Target.FindAttributeSystem(out var attr) && attr.DefinesAttribute(RelativeAttribute),
                ESourceTarget.Source => impactData.SourcedModifier.BaseDerivation.GetSource().FindAttributeSystem(out var attr) && attr.DefinesAttribute(RelativeAttribute),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
