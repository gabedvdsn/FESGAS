using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [Serializable]
    public abstract class AbstractMagnitudeModifierScriptableObject : ScriptableObject, IMagnitudeModifier
    {
        public abstract void Initialize(GameplayEffectSpec spec);
        public abstract float Evaluate(GameplayEffectSpec spec);
    }
    
    public enum ECalculationOperation
    {
        Add,
        Multiply,
        Override
    }
}
