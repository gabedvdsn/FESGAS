using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [Serializable]
    public abstract class AbstractMagnitudeModifier : IMagnitudeModifier
    {
        public abstract void Initialize(IAttributeImpactDerivation spec);
        
        public abstract float Evaluate(IAttributeImpactDerivation spec);
    }
    
    public enum ECalculationOperation
    {
        Add,
        Multiply,
        Override
    }
}
