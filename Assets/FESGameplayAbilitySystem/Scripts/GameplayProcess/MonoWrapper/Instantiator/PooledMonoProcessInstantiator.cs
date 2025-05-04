using UnityEngine;

namespace FESGameplayAbilitySystem.Instantiator
{
    [CreateAssetMenu(fileName = "MPI_Pooled_", menuName = "FESGAS/Process/Instantiator", order = 0)]
    public class PooledMonoProcessInstantiator : AbstractMonoProcessInstantiatorScriptableObject
    {

        public override AbstractMonoProcess InstantiateProcess(AbstractMonoProcess process, Vector3 initialPosition, Quaternion initialRotation, Transform parentTransform = null)
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
