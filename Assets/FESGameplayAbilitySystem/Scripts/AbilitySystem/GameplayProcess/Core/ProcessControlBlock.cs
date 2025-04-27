using System;
using System.Threading;
using Cysharp.Threading.Tasks;
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
        private EProcessState queuedState;

        public float Lifetime => lifetime;
        private float lifetime;

        public bool IsInitialized => isInitialized;
        private bool isInitialized;

        private CancellationTokenSource cts;

        private ProcessControlBlock(int cacheIndex, int stepIndex, IGameplayProcess process, IGameplayProcessHandler handler)
        {
            CacheIndex = cacheIndex;
            StepIndex = stepIndex;
            
            Process = process;
            Handler = handler;
            
            State = EProcessState.Created;
            queuedState = EProcessState.Waiting;

            lifetime = 0;
        }

        public static ProcessControlBlock Generate(int cacheIndex, int loopingIndex, IGameplayProcess process, IGameplayProcessHandler handler)
        {
            return new ProcessControlBlock(cacheIndex, loopingIndex, process, handler);
        }

        public void Initialize()
        {
            if (isInitialized) return;

            isInitialized = true;
            Process.WhenInitialize();

            State = queuedState;
        }
        
        /// <summary>
        /// FROM: Waiting (Not active), Paused (Active)
        /// </summary>
        public void Run()
        {
            if (State == EProcessState.Running) return;
            queuedState = EProcessState.Running;
            if (State != EProcessState.Paused) cts = new CancellationTokenSource();
            SetQueuedState();
        }

        public void Wait()
        {
            queuedState = EProcessState.Waiting;
            if (State != EProcessState.Running && State != EProcessState.Paused) SetQueuedState();
        }

        public bool Pause()
        {
            if (State != EProcessState.Running) return false;
            queuedState = EProcessState.Paused;
            SetQueuedState();
            return true;
        }

        public bool Cancel()
        {
            if (State != EProcessState.Running && State != EProcessState.Paused) return false;
            if (!cts.Token.IsCancellationRequested) cts.Cancel();
            cts.Dispose();
            cts = null;
            return true;
        }

        public void Terminate()
        {
            queuedState = EProcessState.Terminated;
            if (State != EProcessState.Running) SetQueuedState();
        }

        private void SetQueuedState()
        {
            if (queuedState == State)
            {
                return;
            }

            State = queuedState;
            switch (State)
            {

                case EProcessState.Created:
                    break;
                case EProcessState.Running:
                    RunProcess().Forget();
                    break;
                case EProcessState.Paused:
                    Process.WhenPause();
                    if (!cts.IsCancellationRequested) cts.Cancel();
                    break;
                case EProcessState.Waiting:
                    Process.WhenWait();
                    ProcessControl.Instance.ProcessIsWaiting(this);
                    break;
                case EProcessState.Terminated:
                    Process.WhenTerminate();
                    ProcessControl.Instance.Unregister(this);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        // Cannot be called in isolation, must be followed by a ProcessControl.Terminate(cacheIndex) call
        public void TerminateImmediately()
        {
            if (State != EProcessState.Running) return;
            
            cts.Cancel();
            cts.Dispose();
            cts = null;
            
            queuedState = EProcessState.Terminated;
        }
        
        public void SetStepIndex(int stepIndex) => StepIndex = stepIndex;

        public void Step()
        {
            lifetime += Time.deltaTime;
            Process.WhenUpdate(lifetime);
        }
        
        private async UniTask RunProcess()
        {
            State = EProcessState.Running;

            try
            {
                await Process.RunProcess(cts.Token);
            }
            catch (OperationCanceledException)
            {
                // Paused or terminated
                if (queuedState == EProcessState.Terminated) SetQueuedState();
                return;
            }
            
            if (queuedState != EProcessState.Paused)
            {
                switch (Process.Lifecycle)
                {
                    case EProcessLifecycle.SelfTerminating:
                        Terminate();
                        break;
                    case EProcessLifecycle.RunThenWait:
                    case EProcessLifecycle.DependentRunning:
                        Wait();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            SetQueuedState();
        }

        public ProcessRelay GetRelay()
        {
            return new ProcessRelay(this);
        }
    }

    public struct ProcessRelay
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
        public float Lifetime => pcb.Lifetime;
    }
    
    public enum EProcessState
    {
        Created,  // Created but initialized
        Running,  // Actively running
        Paused,  // Paused during running
        Waiting,  // Ready but cannot run
        Terminated  // Must be terminated
    }
}
