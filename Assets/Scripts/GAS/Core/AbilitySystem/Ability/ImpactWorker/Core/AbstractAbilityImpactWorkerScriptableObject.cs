using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public abstract class AbstractAbilityImpactWorkerScriptableObject : ScriptableObject
    {
        public abstract void InterpretImpact(AbilityImpactData impactData);
    }
}
