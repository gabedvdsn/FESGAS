using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public abstract class AbstractMonoProcessInstantiatorScriptableObject : ScriptableObject
    {
        public abstract AbstractMonoProcess InstantiateProcess(AbstractMonoProcess process, Vector3 initialPosition, Quaternion initialRotation, Transform parentTransform = null);

        public abstract void CleanProcess(AbstractMonoProcess process);
    }
}
