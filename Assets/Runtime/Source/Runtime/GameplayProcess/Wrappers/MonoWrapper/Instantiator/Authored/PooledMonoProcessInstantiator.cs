using UnityEngine;

namespace FESGameplayAbilitySystem.Instantiator
{
    [CreateAssetMenu(fileName = "MPI_Pooled_", menuName = "FESGAS/Process/ObjectPool Instantiator", order = 0)]
    public class PooledMonoProcessInstantiator : AbstractMonoProcessInstantiatorScriptableObject
    {
        protected override AbstractMonoProcess PrepareNew(AbstractMonoProcess process, ProcessDataPacket data)
        {
            // Pooling logic
            return null;
        }
        
        protected override AbstractMonoProcess PrepareExisting(AbstractMonoProcess process, ProcessDataPacket data)
        {
            // Pooling logic
            return null;
        }
        
        public override void CleanProcess(AbstractMonoProcess process)
        {
            // Pooling logic
        }
    }
}
