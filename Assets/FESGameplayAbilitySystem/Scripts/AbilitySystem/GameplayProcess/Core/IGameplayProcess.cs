using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public interface IGameplayProcess
    {
        public void WhenInitialize();  // Called once when the process is first moved to Ready state
        
        public void WhenUpdate(float lifespan);  // Called whenever the PCB is updated
        public void WhenWait();  // Called whenever the process is set to Wait state
        public void WhenTerminate();  // Called whenever the process is terminated

        public UniTask RunProcess(CancellationToken token);
        
        public int StepPriority { get; }
        public EProcessUpdateTiming StepTiming { get; }
        public EProcessLifecycle Lifecycle { get; }
    }

    public enum EProcessUpdateTiming
    {
        None = 0,
        Update = 1,
        LateUpdate = 2,
        FixedUpdate = 3
    }

    public enum EProcessLifecycle
    {
        SelfTerminating,
        RunThenWait,
        DependentRunning
    }
}
