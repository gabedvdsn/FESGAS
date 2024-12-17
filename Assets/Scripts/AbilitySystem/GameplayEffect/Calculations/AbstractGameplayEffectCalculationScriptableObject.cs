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
        public abstract float Evaluate(AbilitySystemComponent target, AbilitySystemComponent source, GameplayEffectSpec gameplayEffect);
    }
}
