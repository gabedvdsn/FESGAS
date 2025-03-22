using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public abstract class AbstractImpactWorkerScriptableObject : ScriptableObject
    {
        public abstract void InterpretImpact(AbilityImpactData impactData);

        public abstract bool ValidateWorkFor(AbilityImpactData impactData);
    }
}
