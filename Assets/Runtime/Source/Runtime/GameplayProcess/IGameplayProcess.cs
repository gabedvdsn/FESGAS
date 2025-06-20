using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public interface IGameplayProcess
    {
        public void WhenInitialize(ProcessRelay relay);  // Called once when the process is first moved to Ready state
        
        public void WhenUpdate(EProcessUpdateTiming timing, ProcessRelay relay);  // Called whenever the PCB is updated
        public void WhenWait(ProcessRelay relay);  // Called whenever the process is set to Wait state
        public void WhenTerminate(ProcessRelay relay);  // Called whenever the process is terminated
        public void WhenTerminateSafe(ProcessRelay relay);  // Called whenever an adjacent process is terminated (typically for MonoProcesses, such that the GO is not destroyed twice)

        public UniTask RunProcess(ProcessRelay relay, CancellationToken token);

        public bool TryGetProcess<T>(out T process);

        public bool IsInitialized { get; }
        public string ProcessName { get; }
        public int StepPriority { get; }
        public EProcessUpdateTiming StepTiming { get; }
        public EProcessLifecycle Lifecycle { get; }
    }

    public enum EProcessUpdateTiming
    {
        None = 0,
        Update = 1,
        LateUpdate = 2,
        FixedUpdate = 3,
        UpdateAndLate = 4,
        UpdateAndFixed = 5,
        LateAndFixed = 6
    }

    public enum EProcessLifecycle
    {
        SelfTerminating,
        RunThenWait,
        RequiresControl
    }
}
