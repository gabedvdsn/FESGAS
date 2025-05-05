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
        
        public void WhenUpdate(ProcessRelay relay);  // Called whenever the PCB is updated
        public void WhenWait(ProcessRelay relay);  // Called whenever the process is set to Wait state
        public void WhenTerminate(ProcessRelay relay);  // Called whenever the process is terminated

        public UniTask RunProcess(ProcessRelay relay, CancellationToken token);

        public GameplayTagScriptableObject ProcessTag { get; }
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
        RequiresControl
    }
}
