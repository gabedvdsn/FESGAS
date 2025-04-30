using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public class MonoWrapperProcess : IGameplayProcess
    {
        private AbstractMonoProcessDataScriptableObject MonoData;
        
        private Vector3 initialPosition;
        private Quaternion initialRotation;
        private Transform parentTransform;
        
        private MonoGameplayProcess activeMono;

        public MonoWrapperProcess(AbstractMonoProcessDataScriptableObject monoData, Vector3 initialPosition, Quaternion initialRotation, Transform parentTransform = null)
        {
            MonoData = monoData;
            this.initialPosition = initialPosition;
            this.initialRotation = initialRotation;
            this.parentTransform = parentTransform;
        }

        public void WhenInitialize(ProcessRelay relay)
        {
            activeMono = MonoData.WhenInitialize(initialPosition, initialRotation, parentTransform, relay);
        }

        public void WhenUpdate(ProcessRelay relay)
        {
            MonoData.WhenUpdate(activeMono, relay);
        }
        
        public void WhenWait(ProcessRelay relay)
        {
            MonoData.WhenWait(activeMono, relay);
        }

        public void WhenTerminate(ProcessRelay relay)
        {
            MonoData.WhenTerminate(activeMono, relay);
        }
        
        public async UniTask RunProcess(ProcessRelay relay, CancellationToken token)
        {
            await MonoData.RunProcess(activeMono, relay, token);
        }
        
        public int StepPriority => MonoData.StepPriority;
        public EProcessUpdateTiming StepTiming => MonoData.StepTiming;
        public EProcessLifecycle Lifecycle => MonoData.Lifecycle;
    }
}
