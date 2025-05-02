using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public class MonoWrapperProcess : IGameplayProcess
    {
        private AbstractMonoProcess MonoPrefab;

        private Vector3 initialPosition;
        private Quaternion initialRotation;
        private Transform parentTransform;
        
        private AbstractMonoProcess activeMono;

        public MonoWrapperProcess(AbstractMonoProcess monoPrefab)
        {
            MonoPrefab = monoPrefab;
        }

        public MonoWrapperProcess(AbstractMonoProcess monoPrefab, Vector3 initialPosition, Quaternion initialRotation, Transform parentTransform = null)
        {
            MonoPrefab = monoPrefab;
            this.initialPosition = initialPosition;
            this.initialRotation = initialRotation;
            this.parentTransform = parentTransform;
        }

        public void SetPosition(Vector3 pos) => initialPosition = pos;
        public void SetRotation(Quaternion rot) => initialRotation = rot;
        public void SetParentTransform(Transform pt) => parentTransform = pt;

        public void WhenInitialize(ProcessRelay relay)
        {
            if (parentTransform is null) activeMono = Object.Instantiate(MonoPrefab, initialPosition, initialRotation);
            else activeMono = Object.Instantiate(MonoPrefab, initialPosition, initialRotation, parentTransform);
            
            activeMono.WhenInitialize(relay);
        }

        public void WhenUpdate(ProcessRelay relay)
        {
            activeMono.WhenUpdate(relay);
        }
        
        public void WhenWait(ProcessRelay relay)
        {
            activeMono.WhenWait(relay);
        }

        public void WhenTerminate(ProcessRelay relay)
        {
            activeMono.WhenTerminate(relay);
        }
        
        public async UniTask RunProcess(ProcessRelay relay, CancellationToken token)
        {
            await activeMono.RunProcess(relay, token);
        }
        
        public int StepPriority => activeMono.StepPriority;
        public EProcessUpdateTiming StepTiming => activeMono.StepTiming;
        public EProcessLifecycle Lifecycle => activeMono.Lifecycle;
    }
}
