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

        public void WhenInitialize()
        {
            Debug.Log("Mono process initialized");
            
            activeMono = MonoData.WhenInitialize(initialPosition, initialRotation, parentTransform);
        }

        public void WhenUpdate(float lifespan)
        {
            MonoData.WhenUpdate(activeMono, lifespan);
        }
        
        public void WhenWait()
        {
            MonoData.WhenWait(activeMono);
        }
        public void WhenPause()
        {
            
        }

        public void WhenTerminate()
        {
            Debug.Log("Mono process terminated");
            
            MonoData.WhenTerminate(activeMono);
        }
        
        public async UniTask RunProcess(CancellationToken token)
        {
            await MonoData.RunProcess(activeMono, token);
        }
        
        public int StepPriority => MonoData.StepPriority;
        public EProcessUpdateTiming StepTiming => MonoData.StepTiming;
        public EProcessLifecycle Lifecycle => MonoData.Lifecycle;
    }
}
