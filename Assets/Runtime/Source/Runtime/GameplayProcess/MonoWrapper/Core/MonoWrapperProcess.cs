﻿using System.Threading;
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
            // Check if is an instance
            if (MonoPrefab.gameObject.scene.name is not null)
            {
                activeMono = MonoPrefab;
                activeMono.SendProcessData(DataPacket);
                activeMono.WhenInitialize(relay);
                return;
            }
            
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
            
            DataPacket.AddPayload(GameRoot.TransformTag, ESourceTargetData.Data, GameRoot.Instance.transform);
            activeMono.SendProcessData(DataPacket);
            activeMono.WhenInitialize(relay);
            
            // Register adjacent processes
            foreach (AbstractMonoProcess adjProcess in activeMono.GetComponentsInChildren<AbstractMonoProcess>())
            {
                if (adjProcess == activeMono) continue;
                if (adjProcess.IsInitialized) continue;  // Don't register registered processes
                
                ProcessControl.Instance.Register(adjProcess, DataPacket, out var adjRelay);
                ProcessControl.Instance.AssignAdjacentProcess(relay, adjRelay);
            }
        }

        public void WhenUpdate(EProcessUpdateTiming timing, ProcessRelay relay)
        {
            activeMono.WhenUpdate(relay);
        }
        
        public void WhenWait(ProcessRelay relay)
        {
            activeMono.WhenWait(relay);
        }

        public void WhenTerminate(ProcessRelay relay)
        {
            WhenTerminateSafe(relay);
            if (activeMono is not null) Object.Destroy(activeMono.gameObject);
        }
        public void WhenTerminateSafe(ProcessRelay relay)
        {
            if (!activeMono) return;
            
            activeMono.WhenTerminate(relay);
            if (activeMono.Instantiator is not null)
            {
                activeMono.Instantiator.CleanProcess(activeMono);
            }
        }

        public async UniTask RunProcess(ProcessRelay relay, CancellationToken token)
        {
            await activeMono.RunProcess(relay, token);
        }

        public bool TryGetProcess<T>(out T process)
        {
            if (activeMono is T cast)
            {
                process = cast;
                return true;
            }

            process = default;
            return false;
        }
        public bool IsInitialized => activeMono && activeMono.IsInitialized;

        public string ProcessName => activeMono ? activeMono.name : "[ ]";
        public int StepPriority => activeMono.StepPriority;
        public EProcessUpdateTiming StepTiming => activeMono.StepTiming;
        public EProcessLifecycle Lifecycle => activeMono.Lifecycle;
    }
}
