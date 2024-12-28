using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public abstract class AbstractGameplayEffectScriptableObject : ScriptableObject
    {
        public abstract GameplayEffectSpec Generate(AbilitySpec ability, GASComponent target);
        public abstract void ApplyImpactSpecification(GameplayEffectSpec spec);
    }
}
