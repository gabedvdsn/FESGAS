using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public abstract class AbstractExternalBehaviourScriptableObject : ScriptableObject
    {
        public abstract void Run(GASComponentBase gas);
    }
}
