using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public abstract class AbstractImpactWorkerScriptableObject : ScriptableObject
    {
        [HideInInspector] public string ReadOnlyDescription = "Impact workers are used to interpret and execute with respect to impact data.\nE.g. lifesteal, damage numbers, etc...";
        
        public abstract void InterpretImpact(AbilityImpactData impactData);

        public abstract bool ValidateWorkFor(AbilityImpactData impactData);
    }
}
