using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public abstract class AbstractGameplayEffectScriptableObject : ScriptableObject
    {
        public abstract GameplayEffectSpec Generate(IEffectDerivation derivation, GASComponent target);
        public abstract void ApplyImpactSpecification(GameplayEffectSpec spec);
    }
}
