using System;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public abstract class RelativeOperationContextApplicationWorkerScriptableObject : AbstractImpactContextApplicationWorkerScriptableObject
    {
        [Header("Relative & Operation")] 
        
        public AttributeScriptableObject RelativeAttribute;
        public ECalculationOperation Operation;
        
        protected AttributeValue ComputeModifiedImpact(AttributeValue relValue, AttributeValue impactValue)
        {
            return Operation switch
            {
                ECalculationOperation.Add => impactValue + relValue,
                ECalculationOperation.Multiply => impactValue * relValue,
                ECalculationOperation.Override => relValue,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
        public override bool ValidateWorkFor(GASComponentBase target, SourcedModifiedAttributeValue smav)
        {
            return base.ValidateWorkFor(target, smav) && smav.BaseDerivation.GetSource().AttributeSystem.DefinesAttribute(RelativeAttribute);
        }
    }
}
