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
            if (MonoPrefab.Instantiator is not null)
            {
                activeMono = MonoPrefab.Instantiator.InstantiateProcess(MonoPrefab, initialPosition, initialRotation, parentTransform);
                if (activeMono)
                {
                    activeMono.WhenInitialize(relay);
                    activeMono.SendProcessData();
                }
            }
            
            if (parentTransform is null) activeMono = Object.Instantiate(MonoPrefab, initialPosition, initialRotation, GameRoot.Instance ? GameRoot.Instance.transform : null);
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
            if (!activeMono) return;
            
            activeMono.WhenTerminate(relay);
            if (activeMono.Instantiator is not null)
            {
                activeMono.Instantiator.CleanProcess(activeMono);
            }
            if (activeMono is not null) Object.Destroy(activeMono.gameObject);
        }
        
        public async UniTask RunProcess(ProcessRelay relay, CancellationToken token)
        {
            await activeMono.RunProcess(relay, token);
        }

        public string ProcessName => activeMono ? activeMono.name : "[ ]";
        public int StepPriority => activeMono.StepPriority;
        public EProcessUpdateTiming StepTiming => activeMono.StepTiming;
        public EProcessLifecycle Lifecycle => activeMono.Lifecycle;
    }
}
