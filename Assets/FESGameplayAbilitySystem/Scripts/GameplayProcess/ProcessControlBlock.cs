using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public class ProcessControlBlock
    {
        public readonly IGameplayProcess Process;
        public readonly IGameplayProcessHandler Handler;

        public readonly int CacheIndex;
        public int StepIndex { get; private set; }
        
        public EProcessState State { get; private set; }
        public EProcessState queuedState { get; private set; }

        public float UnscaledLifetime => Time.unscaledTime - unscaledInitializeTime;
        public float Lifetime => Time.time - initializeTime;
        public float UnscaledInitializeTime => unscaledInitializeTime;
        private float unscaledInitializeTime;
        
        public float InitializeTime => initializeTime;
        private float initializeTime;
        
        public float UpdateTime => updateTime;
        private float updateTime;

        public bool IsInitialized => isInitialized;
        private bool isInitialized;

        public bool HasRun => hasRun;
        private bool hasRun;
        public bool MidRun => midRun;
        private bool midRun;

        private CancellationTokenSource cts;

        protected ProcessRelay relay;

        protected ProcessControlBlock(int cacheIndex, int stepIndex, IGameplayProcess process, IGameplayProcessHandler handler)
        {
            relay = new ProcessRelay(this);

            CacheIndex = cacheIndex;
            StepIndex = stepIndex;
            
            Process = process;
            Handler = handler;
            
            State = EProcessState.Created;

            updateTime = 0;
        }

        public static ProcessControlBlock Generate(int cacheIndex, int stepIndex, IGameplayProcess process, IGameplayProcessHandler handler)
        {
            return new ProcessControlBlock(cacheIndex, stepIndex, process, handler);
        }

        public void ForceIntoState(EProcessState state)
        {
            if (State == EProcessState.Running && state != EProcessState.Running) Interrupt();
            queuedState = state;
            SetQueuedState();
        }
        
        public void QueueNextState(EProcessState state)
        {
            if (state == State) return;
            
            switch (state)
            {
                case EProcessState.Created:
                    break;
                case EProcessState.Running:
                    QueueRun();
                    break;
                case EProcessState.Waiting:
                    QueueWait();
                    break;
                case EProcessState.Terminated:
                    QueueTerminate();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        public void Initialize()
        {
            if (isInitialized) return;

            unscaledInitializeTime = Time.unscaledTime;
            initializeTime = Time.time;

            isInitialized = true;
            Process.WhenInitialize(relay);
        }
        
        private void QueueRun()
        {
            queuedState = EProcessState.Running;
            if (State != EProcessState.Running) SetQueuedState();
        }

        public bool Run()
        {
            if (State != EProcessState.Running) return false;

            RunProcess().Forget();
            queuedState = ProcessControl.Instance.GetDefaultTransitionState(this);
            
            return true;
        }

        private void QueueWait()
        {
            queuedState = EProcessState.Waiting;
            if (State != EProcessState.Running) SetQueuedState();
        }

        public bool Wait()
        {
            if (State != EProcessState.Waiting) return false;

            Process.WhenWait(relay);
            
            return true;
        }

        private void QueueTerminate()
        {
            queuedState = EProcessState.Terminated;
            if (State != EProcessState.Running) SetQueuedState();
        }

        public bool Terminate()
        {
            if (State != EProcessState.Terminated) return false;
            
            Process.WhenTerminate(relay);
            ProcessControl.Instance.Unregister(this);
            
            return true;
        }

        private void SetQueuedState()
        {
            if (queuedState == State) return;
            State = queuedState;
            switch (State)
            {
                case EProcessState.Created:
                    break;
                case EProcessState.Running:
                    ProcessControl.Instance.ProcessWillRun(this);
                    break;
                case EProcessState.Waiting:
                    ProcessControl.Instance.ProcessWillWait(this);
                    break;
                case EProcessState.Terminated:
                    ProcessControl.Instance.ProcessWillTerminate(this);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        // Cannot be called in isolation, must be followed by a ProcessControl.Terminate(cacheIndex) call
        public void Interrupt()
        {
            if (State != EProcessState.Running) return;
            
            cts?.Cancel();
        }

        public void Step()
        {
            Process.WhenUpdate(relay);
            
            updateTime += Time.deltaTime;
            // lifetime += Time.unscaledDeltaTime;
        }
        
        private async UniTask RunProcess()
        {
            cts = new CancellationTokenSource();
            
            hasRun = true;
            
            if (!midRun) updateTime = 0f;
            midRun = false;

            bool set = true;
            try
            {
                await Process.RunProcess(relay, cts.Token);
            }
            catch (OperationCanceledException)
            {
                midRun = true;
                set = false;
            }

            if (!cts.IsCancellationRequested) cts.Cancel();
            cts.Dispose();
            cts = null;

            if (set) SetQueuedState();
        }

        public ProcessRelay GetRelay()
        {
            return relay;
        }
        
        public void SetStepIndex(int stepIndex) => StepIndex = stepIndex;
    }

    public class ProcessRelay
    {
        private ProcessControlBlock pcb;
        
        public bool Valid => pcb is not null;

        public ProcessRelay(ProcessControlBlock pcb)
        {
            this.pcb = pcb;
        }

        public int CacheIndex => pcb.CacheIndex;
        public IGameplayProcess Process => pcb.Process;
        public IGameplayProcessHandler Handler => pcb.Handler;
        public EProcessState State => pcb.State;
        public EProcessState QueuedState => pcb.queuedState;
        public float UnscaledLifetime => pcb.UnscaledLifetime;
        public float Lifetime => pcb.Lifetime;
        public float UpdateTime => pcb.UpdateTime;

        public bool TryGetProcess<T>(out T process)
        {
            return pcb.Process.TryGetProcess(out process);
        }
        
        public int RemainingRuntime(int runtime, int multiplier = 1000) => runtime - (int)UpdateTime * multiplier;
    }
    
    public enum EProcessState
    {
        Created,  // Created but initialized
        Running,  // Actively running
        Waiting,  // Ready but cannot run
        Terminated  // Must be terminated
    }
}
