using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [Serializable]
    public abstract class AbstractGameplayEffectCalculationScriptableObject : ScriptableObject
    {
        public abstract void Initialize(GameplayEffectSpec spec);
        public abstract float Evaluate(GameplayEffectSpec spec);
    }
    
    public enum CalculationOperation
    {
        Add,
        Multiply,
        Override
    }
}
