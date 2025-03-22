using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public abstract class AbstractApplicationWorkerScriptableObject : ScriptableObject
    {
        public abstract SourcedModifiedAttributeValue ModifyImpact(GASComponent target, SourcedModifiedAttributeValue smav);

        public abstract bool ValidateWorkFor(GASComponent target, SourcedModifiedAttributeValue smav);
    }
}
