using UnityEngine;

namespace FESGameplayAbilitySystem.Core.AttributeSystem.AttributeWorker.Core
{
    public abstract class AbstractAttributeWorkerScriptableObject : ScriptableObject
    {
        public abstract void Activate(AttributeSystemComponent system);
    }
}
