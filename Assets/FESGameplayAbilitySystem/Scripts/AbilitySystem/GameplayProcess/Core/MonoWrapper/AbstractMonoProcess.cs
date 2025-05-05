using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public abstract class AbstractMonoProcess : MonoBehaviour
    {
        [Header("Mono Gameplay Process")] 
        
        public int ProcessStepPriority;
        public EProcessLifecycle ProcessLifecycle;
        public EProcessUpdateTiming ProcessTiming;
        
        [Space(5)]
        
        [Tooltip("Uses Object.Instantiate when null")]
        public AbstractMonoProcessInstantiatorScriptableObject Instantiator;
        
        public abstract void WhenInitialize(ProcessRelay relay);
        
        public abstract void WhenUpdate(ProcessRelay relay);
        
        public abstract void WhenWait(ProcessRelay relay);
        
        public abstract void WhenTerminate(ProcessRelay relay);
        
        public abstract UniTask RunProcess(ProcessRelay relay, CancellationToken token);

        public int StepPriority => ProcessStepPriority;
        public EProcessUpdateTiming StepTiming => ProcessTiming;
        public EProcessLifecycle Lifecycle => ProcessLifecycle;

        private void OnDestroy()
        {
            Debug.Log($"destroyed!?");
        }
    }
}
