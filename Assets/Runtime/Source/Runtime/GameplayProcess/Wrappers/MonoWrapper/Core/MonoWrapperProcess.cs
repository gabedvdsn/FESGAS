using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public class MonoWrapperProcess : AbstractProcessWrapper
    {
        private AbstractMonoProcess MonoPrefab;
        private ProcessDataPacket DataPacket;
        
        private AbstractMonoProcess activeMono;

        public MonoWrapperProcess(AbstractMonoProcess monoPrefab, ProcessDataPacket data)
        {
            MonoPrefab = monoPrefab;
            DataPacket = data;
        }

        public override void InitializeWrapper()
        {
            // Check if is an instance
            if (MonoPrefab.gameObject.scene.name is not null)
            {
                activeMono = MonoPrefab;
                return;
            }
            
            if (MonoPrefab.Instantiator is not null)
            {
                activeMono = MonoPrefab.Instantiator.InstantiateProcess(MonoPrefab, DataPacket);
            }
            else
            {
                activeMono = Object.Instantiate(MonoPrefab);
                activeMono.name = activeMono.name.Replace("(Clone)", "");
                DataPacket.AddPayload(GameRoot.TransformTag, ESourceTargetData.Data, GameRoot.Instance.transform);
            }
        }
        
        public override void WhenInitialize(ProcessRelay relay)
        {
            activeMono.SendProcessData(DataPacket);
            activeMono.WhenInitialize(relay);
            
            // Register as dependant to leader processes
            foreach (AbstractMonoProcess adjProcess in activeMono.GetComponentsInParent<AbstractMonoProcess>())
            {
                if (adjProcess == activeMono) continue;

                ProcessRelay adjRelay;
                if (!adjProcess.IsInitialized) ProcessControl.Instance.Register(adjProcess, DataPacket, out adjRelay);
                else adjRelay = adjProcess.Relay;
                
                
                ProcessControl.Instance.AssignDependant(relay, adjRelay);
            }
        }

        public override void WhenUpdate(EProcessUpdateTiming timing, ProcessRelay relay)
        {
            activeMono.WhenUpdate(relay);
        }
        
        public override void WhenWait(ProcessRelay relay)
        {
            activeMono.WhenWait(relay);
        }

        public override void WhenTerminate(ProcessRelay relay)
        {
            WhenTerminateSafe(relay);
            if (activeMono) Object.Destroy(activeMono.gameObject);
        }
        public override void WhenTerminateSafe(ProcessRelay relay)
        {
            if (!activeMono) return;
            
            activeMono.WhenTerminateSafe(relay);
            if (activeMono.Instantiator is not null)
            {
                activeMono.Instantiator.CleanProcess(activeMono);
            }
        }

        public override async UniTask RunProcess(ProcessRelay relay, CancellationToken token)
        {
            await activeMono.RunProcess(relay, token);
        }

        public override bool TryGetProcess<T>(out T process)
        {
            if (activeMono is T cast)
            {
                process = cast;
                return true;
            }

            process = default;
            return false;
        }

        public override string ProcessName => activeMono ? activeMono.name : "[ ]";
        public override int StepPriority => activeMono.StepPriority;
        public override EProcessUpdateTiming StepTiming => activeMono.StepTiming;
        public override EProcessLifecycle Lifecycle => activeMono.Lifecycle;
    }
}
