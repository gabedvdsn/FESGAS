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
        public readonly AbstractProcessWrapper Process;
        public readonly IGameplayProcessHandler Handler;

        public ProcessRelay Relay => Process?.Relay;

        public readonly int CacheIndex;
        private Dictionary<EProcessUpdateTiming, int> StepIndices;
        public int StepIndex(EProcessUpdateTiming timing) => StepIndices.ContainsKey(timing) ? StepIndices[timing] : -1;

        public Dictionary<int, List<int>> DependantProcesses;  // Composed & child processes (hierarchical)
        
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
        
        protected ProcessControlBlock(int cacheIndex, AbstractProcessWrapper process, IGameplayProcessHandler handler)
        {
            CacheIndex = cacheIndex;
            StepIndices = new Dictionary<EProcessUpdateTiming, int>();
            DependantProcesses = new Dictionary<int, List<int>>()
            {
                { 0, new List<int>() },
                { 1, new List<int>() }
            };
            
            Process = process;
            Handler = handler;

            Process.Relay = new ProcessRelay(this);
            
            State = EProcessState.Created;
            updateTime = 0;
        }

        public static ProcessControlBlock Generate(int cacheIndex, AbstractProcessWrapper process, IGameplayProcessHandler handler)
        {
            return new ProcessControlBlock(cacheIndex, process, handler);
        }

        /// <summary>
        /// If asDependant is TRUE, then the adjacency is assigned as a dependant of this process.
        /// Otherwise, the index is stored to reflect that this process is a dependant of the adjacent process.
        /// </summary>
        /// <param name="asDependant">Whether the adjacent process is a dependant of this process.</param>
        /// <param name="adjIndex">The adjacent process index.</param>
        public void AssignAdjacency(bool asDependant, int adjIndex)
        {
            int key = asDependant ? 0 : 1;
            if (DependantProcesses[key].Contains(adjIndex)) return;
            DependantProcesses[key].Add(adjIndex);
        }

        public async UniTask ForceIntoState(EProcessState state)
        {
            if (State == EProcessState.Running && state != EProcessState.Running) Interrupt();
            foreach (var adjIndex in DependantProcesses[0]) ProcessControl.Instance.ForceSet(adjIndex, state).Forget();
            
            await UniTask.CompletedTask;
            
            queuedState = state;
            SetQueuedState();
        }
        
        public void QueueNextState(EProcessState state)
        {
            if (state == State) return;
            
            foreach (var adjIndex in DependantProcesses[0]) ProcessControl.Instance.Set(adjIndex, state);
            
            switch (state)
            {
                case EProcessState.Created:
                    break;
                case EProcessState.Running:
                case EProcessState.Waiting:
                case EProcessState.Terminated:
                    queuedState = state;
                    if (State != EProcessState.Running) SetQueuedState();
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
            Process.WhenInitialize(Process.Relay);
        }
        
        public bool Run()
        {
            if (State != EProcessState.Running) return false;

            RunProcess().Forget();
            queuedState = ProcessControl.Instance.GetDefaultTransitionState(this);
            
            return true;
        }

        public bool Wait()
        {
            if (State != EProcessState.Waiting) return false;

            Process.WhenWait(Process.Relay);
            
            return true;
        }
        
        public bool Terminate()
        {
            if (State != EProcessState.Terminated) return false;
            
            Process.WhenTerminate(Process.Relay);
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

        public void Step(EProcessUpdateTiming timing)
        {
            Process.WhenUpdate(timing, Process.Relay);
            
            updateTime += Time.deltaTime;
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
                await Process.RunProcess(Process.Relay, cts.Token);
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
        
        public void SetStepIndex(EProcessUpdateTiming timing, int stepIndex)
        {
            StepIndices[timing] = stepIndex;
        }
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
        public List<int> Dependants => pcb.DependantProcesses[0];
        public List<int> Leaders => pcb.DependantProcesses[1];
        public string FormattedDependants => string.Join(',', pcb.DependantProcesses);

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
