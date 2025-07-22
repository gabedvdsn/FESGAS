using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public abstract class AbstractMonoProcessInstantiatorScriptableObject : ScriptableObject
    {
        public abstract AbstractMonoProcess InstantiateProcess(AbstractMonoProcess process, ProcessDataPacket data);

        public abstract void CleanProcess(AbstractMonoProcess process);
    }
}
