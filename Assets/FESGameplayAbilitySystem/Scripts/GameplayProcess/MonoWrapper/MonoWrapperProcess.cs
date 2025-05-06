using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public class MonoWrapperProcess : IGameplayProcess
    {
        private AbstractMonoProcess MonoPrefab;
        private ProcessDataPacket DataPacket;
        
        private AbstractMonoProcess activeMono;

        public MonoWrapperProcess(AbstractMonoProcess monoPrefab, ProcessDataPacket data)
        {
            MonoPrefab = monoPrefab;
            DataPacket = data;
        }

        public void WhenInitialize(ProcessRelay relay)
        {
            if (MonoPrefab.Instantiator is not null)
            {
                activeMono = MonoPrefab.Instantiator.InstantiateProcess(MonoPrefab);
                if (activeMono)
                {
                    activeMono.SendProcessData(DataPacket);
                    activeMono.WhenInitialize(relay);
                    
                    return;
                }
            }
            else
            {
                activeMono = Object.Instantiate(MonoPrefab);
            }
            
            DataPacket.AddPayload(ESourceTargetData.Data, GameRoot.TransformTag, GameRoot.Instance.transform);
            activeMono.SendProcessData(DataPacket);
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
