using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public abstract class AbstractApplicationWorkerScriptableObject : ScriptableObject
    {
        [HideInInspector] public string ReadOnlyDescription = "Application workers are used to modify the values of impacting effects when they are applied to a target.";
        
        public abstract SourcedModifiedAttributeValue ModifyImpact(GASComponentBase target, SourcedModifiedAttributeValue smav);

        public abstract bool ValidateWorkFor(GASComponentBase target, SourcedModifiedAttributeValue smav);
    }
}
