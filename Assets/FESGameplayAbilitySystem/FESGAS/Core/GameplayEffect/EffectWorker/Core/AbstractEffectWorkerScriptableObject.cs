using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public abstract class AbstractEffectWorkerScriptableObject : ScriptableObject
    {
        public abstract void OnEffectApplication(IAttributeImpactDerivation derivation);
        public abstract void OnEffectTick(IAttributeImpactDerivation derivation);
        public abstract void OnEffectRemoval(IAttributeImpactDerivation derivation);
        public abstract void OnEffectImpact(AbilityImpactData impactData);
    }
}
