using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public class ProcessControl : MonoBehaviour, IGameplayProcessHandler
    {
        // Singleton instance
        public static ProcessControl Instance;

        [Header("Process Control")] 
        
        public EProcessControlState StartState = EProcessControlState.Ready;
        public new bool DontDestroyOnLoad = true;
        
        public EProcessControlState State { get; private set; }

        private Dictionary<int, ProcessControlBlock> active = new();
        private Dictionary<EProcessUpdateTiming, SortedDictionary<int, List<int>>> stepping;
        private HashSet<int> waiting = new();

        private int cacheCounter = 0;
        private int NextCacheIndex => cacheCounter++;

        public int Created => cacheCounter;
        public int Active => active.Count;

        #region Events
        
        private void Awake()
        {
            if (Instance is not null && Instance != this)
            {
                Destroy(gameObject);
            }

            Instance = this;
            if (DontDestroyOnLoad) DontDestroyOnLoad(gameObject);
            
            ResetProcessControl(StartState);
        }

        private void Update()
        {
            Step(EProcessUpdateTiming.Update);
        }

        private void LateUpdate()
        {
            Step(EProcessUpdateTiming.LateUpdate);
        }

        private void FixedUpdate()
        {
            Step(EProcessUpdateTiming.FixedUpdate);
        }

        private void Step(EProcessUpdateTiming timing)
        {
            if (State is EProcessControlState.Waiting or EProcessControlState.TerminatedImmediately) return;
            
            foreach (var priority in stepping[timing])
            {
                foreach (var cacheIndex in priority.Value) active[cacheIndex].Step(timing);
            }
        }
        
        #endregion

        #region Core
        
        public void SetControlState(EProcessControlState state)
        {
            if (state == State) return;
            
            State = state;

            SetAllProcessesUponStateChange();
        }

        private void ResetProcessControl(EProcessControlState nextState)
        {
            TerminateAllImmediately();

            active = new Dictionary<int, ProcessControlBlock>();
            stepping = new Dictionary<EProcessUpdateTiming, SortedDictionary<int, List<int>>>();
            foreach (EProcessUpdateTiming timing in Enum.GetValues(typeof(EProcessUpdateTiming)))
            {
                stepping[timing] = new SortedDictionary<int, List<int>>();
            }
            waiting = new HashSet<int>();
            
            State = nextState;
        }
        
        // Register a new process and handler to a PCB
        public bool Register(IGameplayProcess process, IGameplayProcessHandler handler, out ProcessRelay relay)
        {
            relay = default;
            if (State is EProcessControlState.Closed 
                or EProcessControlState.ClosedWaiting 
                or EProcessControlState.Terminated 
                or EProcessControlState.TerminatedImmediately) return false;
            
            var pcb = ProcessControlBlock.Generate(
                NextCacheIndex,
                process, handler
            );

            SetProcess(pcb);
            
            relay = pcb.GetRelay();
            handler?.HandlerSubscribeProcess(relay);
            
            return true;
        }
        
        public bool Register(AbstractMonoProcess process, ProcessDataPacket data, out ProcessRelay relay)
        {
            return Register
            (
                new MonoWrapperProcess(process, data),
                data.Handler,
                out relay
            );
        }
        
        // Unregister a PCB
        public bool Unregister(ProcessControlBlock pcb)
        {
            if (waiting.Contains(pcb.CacheIndex)) waiting.Remove(pcb.CacheIndex);
            else RemoveFromStepping(pcb);

            pcb.Handler?.HandlerVoidProcess(pcb.CacheIndex);
            
            return active.Remove(pcb.CacheIndex);
        }

        public void AssignAdjacentProcess(ProcessRelay source, ProcessRelay adjacent)
        {
            active[source.CacheIndex].AssignAdjacency(adjacent.CacheIndex);
        }

        public Dictionary<int, ProcessControlBlock> FetchActiveProcesses()
        {
            return active;
        }
        
        #endregion
        
        #region Control

        public bool Run(int cacheIndex)
        {
            if (!active.ContainsKey(cacheIndex)) return false;
            if (!ValidatePCBStateTransfer(active[cacheIndex], EProcessState.Running)) return false;

            SetProcess(active[cacheIndex]);
            return true;
        }
        
        public bool Wait(int cacheIndex)
        {
            if (!active.ContainsKey(cacheIndex)) return false;
            if (!ValidatePCBStateTransfer(active[cacheIndex], EProcessState.Waiting)) return false;
            
            active[cacheIndex].QueueNextState(EProcessState.Waiting);
            return true;
        }
        
        public bool Terminate(int cacheIndex)
        {
            if (!active.ContainsKey(cacheIndex)) return false;
            
            active[cacheIndex].QueueNextState(EProcessState.Terminated);
            return true;
        }

        public async UniTask TerminateImmediate(int cacheIndex)
        {
            if (!active.ContainsKey(cacheIndex))
            {
                await UniTask.CompletedTask;
                return;
            }
            
            await active[cacheIndex].ForceIntoState(EProcessState.Terminated);
        }
        
        public void TerminateAll()
        {
            List<int> indices = active.Keys.ToList();
            foreach (int cacheIndex in indices) Terminate(cacheIndex);
        }

        public async UniTask TerminateAllImmediately()
        {
            await UniTask.WhenAll(active.Keys.Select(index => active[index].ForceIntoState(EProcessState.Terminated)));
        }
        
        #endregion

        #region Process Setting
        
        private void SetProcess(ProcessControlBlock pcb)
        {
            if (pcb.State == EProcessState.Created) PrepareCreatedProcess();
            var state = GetDefaultTransitionState(pcb);
            
            pcb.QueueNextState(state);
            
            return;

            void PrepareCreatedProcess()
            {
                pcb.Initialize();

                waiting.Add(pcb.CacheIndex);
                active[pcb.CacheIndex] = pcb;
            }
        }
        
        private void SetAllProcessesUponStateChange()
        {
            if (State == EProcessControlState.TerminatedImmediately) TerminateAllImmediately();
            else if (State == EProcessControlState.Terminated) TerminateAll();
            else
            {
                foreach (var pcb in active.Values)
                {
                    if (pcb.State == EProcessState.Created) SetProcess(pcb);
                    else
                    {
                        var setState = GetDefaultStateWhenControlChanged(pcb);
                        if (State is EProcessControlState.Ready or EProcessControlState.Waiting or EProcessControlState.ClosedWaiting) pcb.ForceIntoState(setState);
                        else pcb.QueueNextState(setState);
                    }
                }
            }
        }

        private void MoveToStepping(ProcessControlBlock pcb)
        {
            var timing = pcb.Process.StepTiming;
            int priority = pcb.Process.StepPriority;

            switch (timing)
            {

                case EProcessUpdateTiming.None:
                case EProcessUpdateTiming.Update:
                case EProcessUpdateTiming.LateUpdate:
                case EProcessUpdateTiming.FixedUpdate:
                    SetStepping(pcb, priority, timing);
                    break;
                case EProcessUpdateTiming.UpdateAndLate:
                    SetStepping(pcb, priority, EProcessUpdateTiming.Update);
                    SetStepping(pcb, priority, EProcessUpdateTiming.LateUpdate);
                    break;
                case EProcessUpdateTiming.UpdateAndFixed:
                    SetStepping(pcb, priority, EProcessUpdateTiming.Update);
                    SetStepping(pcb, priority, EProcessUpdateTiming.FixedUpdate);
                    break;
                case EProcessUpdateTiming.LateAndFixed:
                    SetStepping(pcb, priority, EProcessUpdateTiming.LateUpdate);
                    SetStepping(pcb, priority, EProcessUpdateTiming.FixedUpdate);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private void SetStepping(ProcessControlBlock pcb, int priority, EProcessUpdateTiming timing)
        {
            if (!stepping[timing].ContainsKey(priority))
            {
                stepping[timing][priority] = new List<int>();
            }
            
            pcb.SetStepIndex(timing, stepping[timing][priority].Count);
            stepping[timing][priority].Add(pcb.CacheIndex);
        }
        
        private void RemoveFromStepping(ProcessControlBlock pcb)
        {
            //if (pcb.StepIndex < 0) return;
            
            var timing = pcb.Process.StepTiming;
            int priority = pcb.Process.StepPriority;

            switch (timing)
            {

                case EProcessUpdateTiming.None:
                case EProcessUpdateTiming.Update:
                case EProcessUpdateTiming.LateUpdate:
                case EProcessUpdateTiming.FixedUpdate:
                    RemoveStepping(pcb, priority, timing);
                    break;
                case EProcessUpdateTiming.UpdateAndLate:
                    RemoveStepping(pcb, priority, EProcessUpdateTiming.Update);
                    RemoveStepping(pcb, priority, EProcessUpdateTiming.LateUpdate);
                    break;
                case EProcessUpdateTiming.UpdateAndFixed:
                    RemoveStepping(pcb, priority, EProcessUpdateTiming.Update);
                    RemoveStepping(pcb, priority, EProcessUpdateTiming.FixedUpdate);
                    break;
                case EProcessUpdateTiming.LateAndFixed:
                    RemoveStepping(pcb, priority, EProcessUpdateTiming.LateUpdate);
                    RemoveStepping(pcb, priority, EProcessUpdateTiming.FixedUpdate);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
        }

        private void RemoveStepping(ProcessControlBlock pcb, int priority, EProcessUpdateTiming timing)
        {
            int stepIndex = pcb.StepIndex(timing);
            if (stepIndex < 0) return;
            
            int lastIndex = stepping[timing][priority].Count - 1;
            
            if (stepIndex != lastIndex)
            {
                int lastCacheIndex = stepping[timing][priority][lastIndex];
                stepping[timing][priority][stepIndex] = lastCacheIndex;
                active[lastCacheIndex].SetStepIndex(timing, stepIndex);
            }

            stepping[timing][priority].RemoveAt(lastIndex);

            if (stepping[timing][priority].Count == 0) stepping[timing].Remove(priority);
        }

        public void SetWaitingStepIndex(ProcessControlBlock pcb)
        {
            switch (pcb.Process.StepTiming)
            {
                case EProcessUpdateTiming.None:
                case EProcessUpdateTiming.Update:
                case EProcessUpdateTiming.LateUpdate:
                case EProcessUpdateTiming.FixedUpdate:
                    pcb.SetStepIndex(pcb.Process.StepTiming, -1);
                    break;
                case EProcessUpdateTiming.UpdateAndLate:
                    pcb.SetStepIndex(EProcessUpdateTiming.Update, -1);
                    pcb.SetStepIndex(EProcessUpdateTiming.LateUpdate, -1);
                    break;
                case EProcessUpdateTiming.UpdateAndFixed:
                    pcb.SetStepIndex(EProcessUpdateTiming.Update, -1);
                    pcb.SetStepIndex(EProcessUpdateTiming.FixedUpdate, -1);
                    break;
                case EProcessUpdateTiming.LateAndFixed:
                    pcb.SetStepIndex(EProcessUpdateTiming.LateUpdate, -1);
                    pcb.SetStepIndex(EProcessUpdateTiming.FixedUpdate, -1);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private void MoveToWaiting(ProcessControlBlock pcb)
        {
            waiting.Add(pcb.CacheIndex);
            SetWaitingStepIndex(pcb);
        }

        private void RemoveFromWaiting(ProcessControlBlock pcb)
        {
            waiting.Remove(pcb.CacheIndex);
        }
        
        #endregion
        
        #region IPC
        
        public void ProcessWillRun(ProcessControlBlock pcb)
        {
            RemoveFromWaiting(pcb);
            MoveToStepping(pcb);

            pcb.Run();
        }

        public void ProcessWillWait(ProcessControlBlock pcb)
        {
            RemoveFromStepping(pcb);
            MoveToWaiting(pcb);

            pcb.Wait();
        }

        public void ProcessWillTerminate(ProcessControlBlock pcb)
        {
            pcb.Terminate();
        }
        
        #endregion
        
        #region PCB State Transfers

        public EProcessState GetDefaultTransitionState(ProcessControlBlock pcb)
        {
            var lifecycle = pcb.Process.Lifecycle;
            var from = pcb.State;

            return State switch
            {
                EProcessControlState.Ready => lifecycle switch
                {
                    EProcessLifecycle.SelfTerminating => from switch
                    {
                        EProcessState.Created => EProcessState.Running,
                        EProcessState.Running => EProcessState.Terminated,
                        EProcessState.Waiting => EProcessState.Waiting,
                        EProcessState.Terminated => EProcessState.Terminated,
                        _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
                    },
                    EProcessLifecycle.RunThenWait => from switch
                    {
                        EProcessState.Created => EProcessState.Running,
                        EProcessState.Running => EProcessState.Waiting,
                        EProcessState.Waiting => EProcessState.Running,
                        EProcessState.Terminated => EProcessState.Terminated,
                        _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
                    },
                    EProcessLifecycle.RequiresControl => from switch
                    {
                        EProcessState.Created => EProcessState.Waiting,
                        EProcessState.Running => EProcessState.Waiting,
                        EProcessState.Waiting => EProcessState.Running,
                        EProcessState.Terminated => EProcessState.Terminated,
                        _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
                    },
                    _ => throw new ArgumentOutOfRangeException(nameof(lifecycle), lifecycle, null)
                },
                EProcessControlState.Waiting => lifecycle switch
                {
                    EProcessLifecycle.SelfTerminating => from switch
                    {
                        EProcessState.Created => EProcessState.Waiting,
                        EProcessState.Running => EProcessState.Waiting,
                        EProcessState.Waiting => EProcessState.Waiting,
                        EProcessState.Terminated => EProcessState.Terminated,
                        _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
                    },
                    EProcessLifecycle.RunThenWait => from switch
                    {
                        EProcessState.Created => EProcessState.Waiting,
                        EProcessState.Running => EProcessState.Waiting,
                        EProcessState.Waiting => EProcessState.Waiting,
                        EProcessState.Terminated => EProcessState.Terminated,
                        _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
                    },
                    EProcessLifecycle.RequiresControl => from switch
                    {
                        EProcessState.Created => EProcessState.Waiting,
                        EProcessState.Running => EProcessState.Waiting,
                        EProcessState.Waiting => EProcessState.Waiting,
                        EProcessState.Terminated => EProcessState.Terminated,
                        _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
                    },
                    _ => throw new ArgumentOutOfRangeException(nameof(lifecycle), lifecycle, null)
                },
                EProcessControlState.Closed => lifecycle switch
                {
                    EProcessLifecycle.SelfTerminating => from switch
                    {
                        EProcessState.Running => EProcessState.Terminated,
                        _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
                    },
                    EProcessLifecycle.RunThenWait => from switch
                    {
                        EProcessState.Running => EProcessState.Waiting,
                        EProcessState.Waiting => EProcessState.Running,
                        _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
                    },
                    EProcessLifecycle.RequiresControl => from switch
                    {
                        EProcessState.Waiting => EProcessState.Running,
                        EProcessState.Running => EProcessState.Waiting,
                        _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
                    },
                    _ => throw new ArgumentOutOfRangeException(nameof(lifecycle), lifecycle, null)
                },
                EProcessControlState.ClosedWaiting => lifecycle switch
                {
                    EProcessLifecycle.SelfTerminating => from switch
                    {
                        _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
                    },
                    EProcessLifecycle.RunThenWait => from switch
                    {
                        _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
                    },
                    EProcessLifecycle.RequiresControl => from switch
                    {
                        _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
                    },
                    _ => throw new ArgumentOutOfRangeException(nameof(lifecycle), lifecycle, null)
                },
                EProcessControlState.Terminated => lifecycle switch
                {
                    EProcessLifecycle.SelfTerminating => from switch
                    {
                        EProcessState.Created => EProcessState.Terminated,
                        EProcessState.Running => EProcessState.Terminated,
                        EProcessState.Waiting => EProcessState.Terminated,
                        EProcessState.Terminated => EProcessState.Terminated,
                        _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
                    },
                    EProcessLifecycle.RunThenWait => from switch
                    {
                        EProcessState.Created => EProcessState.Terminated,
                        EProcessState.Running => EProcessState.Terminated,
                        EProcessState.Waiting => EProcessState.Terminated,
                        EProcessState.Terminated => EProcessState.Terminated,
                        _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
                    },
                    EProcessLifecycle.RequiresControl => from switch
                    {
                        EProcessState.Created => EProcessState.Terminated,
                        EProcessState.Running => EProcessState.Waiting,
                        EProcessState.Waiting => EProcessState.Terminated,
                        EProcessState.Terminated => EProcessState.Terminated,
                        _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
                    },
                    _ => throw new ArgumentOutOfRangeException(nameof(lifecycle), lifecycle, null)
                },
                EProcessControlState.TerminatedImmediately => lifecycle switch
                {
                    EProcessLifecycle.SelfTerminating => from switch
                    {
                        EProcessState.Created => EProcessState.Terminated,
                        EProcessState.Running => EProcessState.Terminated,
                        EProcessState.Waiting => EProcessState.Terminated,
                        EProcessState.Terminated => EProcessState.Terminated,
                        _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
                    },
                    EProcessLifecycle.RunThenWait => from switch
                    {
                        EProcessState.Created => EProcessState.Terminated,
                        EProcessState.Running => EProcessState.Terminated,
                        EProcessState.Waiting => EProcessState.Terminated,
                        EProcessState.Terminated => EProcessState.Terminated,
                        _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
                    },
                    EProcessLifecycle.RequiresControl => from switch
                    {
                        EProcessState.Created => EProcessState.Terminated,
                        EProcessState.Running => EProcessState.Waiting,
                        EProcessState.Waiting => EProcessState.Terminated,
                        EProcessState.Terminated => EProcessState.Terminated,
                        _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
                    },
                    _ => throw new ArgumentOutOfRangeException(nameof(lifecycle), lifecycle, null)
                },
                _ => throw new ArgumentOutOfRangeException(nameof(State), State, null)
            };
        }
        
        public EProcessState GetDefaultStateWhenControlChanged(ProcessControlBlock pcb)
        {
            var lifecycle = pcb.Process.Lifecycle;
            var from = pcb.State;

            return State switch
            {
                EProcessControlState.Ready => lifecycle switch
                {
                    EProcessLifecycle.SelfTerminating => from switch
                    {
                        EProcessState.Running => EProcessState.Running,
                        EProcessState.Waiting => EProcessState.Running,
                        EProcessState.Terminated => EProcessState.Terminated,
                        _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
                    },
                    EProcessLifecycle.RunThenWait => from switch
                    {
                        EProcessState.Waiting when !pcb.HasRun || pcb.MidRun => EProcessState.Running,
                        EProcessState.Waiting => EProcessState.Waiting,
                        _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
                    },
                    EProcessLifecycle.RequiresControl => from switch
                    {
                        EProcessState.Waiting when pcb.MidRun => EProcessState.Running,
                        EProcessState.Waiting => EProcessState.Waiting,
                        _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
                    },
                    _ => throw new ArgumentOutOfRangeException(nameof(lifecycle), lifecycle, null)
                },
                EProcessControlState.Waiting => lifecycle switch
                {
                    EProcessLifecycle.SelfTerminating => from switch
                    {
                        EProcessState.Waiting => EProcessState.Waiting,
                        EProcessState.Running => EProcessState.Waiting,
                        _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
                    },
                    EProcessLifecycle.RunThenWait => from switch
                    {
                        EProcessState.Waiting => EProcessState.Waiting,
                        EProcessState.Running => EProcessState.Waiting,
                        _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
                    },
                    EProcessLifecycle.RequiresControl => from switch
                    {
                        EProcessState.Waiting => EProcessState.Waiting,
                        EProcessState.Running => EProcessState.Waiting,
                        _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
                    },
                    _ => throw new ArgumentOutOfRangeException(nameof(lifecycle), lifecycle, null)
                },
                EProcessControlState.Closed => lifecycle switch
                {
                    EProcessLifecycle.SelfTerminating => from switch
                    {
                        EProcessState.Running => EProcessState.Running,
                        EProcessState.Waiting => EProcessState.Waiting,
                        EProcessState.Terminated => EProcessState.Terminated,
                        _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
                    },
                    EProcessLifecycle.RunThenWait => from switch
                    {
                        EProcessState.Running => EProcessState.Running,
                        EProcessState.Waiting => EProcessState.Waiting,
                        EProcessState.Terminated => EProcessState.Terminated,
                        _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
                    },
                    EProcessLifecycle.RequiresControl => from switch
                    {
                        EProcessState.Running => EProcessState.Running,
                        EProcessState.Waiting => EProcessState.Waiting,
                        EProcessState.Terminated => EProcessState.Terminated,
                        _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
                    },
                    _ => throw new ArgumentOutOfRangeException(nameof(lifecycle), lifecycle, null)
                },
                EProcessControlState.ClosedWaiting => lifecycle switch
                {
                    EProcessLifecycle.SelfTerminating => from switch
                    {
                        EProcessState.Waiting => EProcessState.Waiting,
                        EProcessState.Running => EProcessState.Waiting,
                        _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
                    },
                    EProcessLifecycle.RunThenWait => from switch
                    {
                        EProcessState.Waiting => EProcessState.Waiting,
                        EProcessState.Running => EProcessState.Waiting,
                        _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
                    },
                    EProcessLifecycle.RequiresControl => from switch
                    {
                        EProcessState.Waiting => EProcessState.Waiting,
                        EProcessState.Running => EProcessState.Waiting,
                        _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
                    },
                    _ => throw new ArgumentOutOfRangeException(nameof(lifecycle), lifecycle, null)
                },
                EProcessControlState.Terminated => lifecycle switch
                {
                    EProcessLifecycle.SelfTerminating => from switch
                    {
                        EProcessState.Created => EProcessState.Terminated,
                        EProcessState.Running => EProcessState.Terminated,
                        EProcessState.Waiting => EProcessState.Terminated,
                        EProcessState.Terminated => EProcessState.Terminated,
                        _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
                    },
                    EProcessLifecycle.RunThenWait => from switch
                    {
                        EProcessState.Created => EProcessState.Terminated,
                        EProcessState.Running => EProcessState.Terminated,
                        EProcessState.Waiting => EProcessState.Terminated,
                        EProcessState.Terminated => EProcessState.Terminated,
                        _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
                    },
                    EProcessLifecycle.RequiresControl => from switch
                    {
                        EProcessState.Created => EProcessState.Terminated,
                        EProcessState.Running => EProcessState.Waiting,
                        EProcessState.Waiting => EProcessState.Terminated,
                        EProcessState.Terminated => EProcessState.Terminated,
                        _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
                    },
                    _ => throw new ArgumentOutOfRangeException(nameof(lifecycle), lifecycle, null)
                },
                EProcessControlState.TerminatedImmediately => lifecycle switch
                {
                    EProcessLifecycle.SelfTerminating => from switch
                    {
                        EProcessState.Created => EProcessState.Terminated,
                        EProcessState.Running => EProcessState.Terminated,
                        EProcessState.Waiting => EProcessState.Terminated,
                        EProcessState.Terminated => EProcessState.Terminated,
                        _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
                    },
                    EProcessLifecycle.RunThenWait => from switch
                    {
                        EProcessState.Created => EProcessState.Terminated,
                        EProcessState.Running => EProcessState.Terminated,
                        EProcessState.Waiting => EProcessState.Terminated,
                        EProcessState.Terminated => EProcessState.Terminated,
                        _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
                    },
                    EProcessLifecycle.RequiresControl => from switch
                    {
                        EProcessState.Created => EProcessState.Terminated,
                        EProcessState.Running => EProcessState.Waiting,
                        EProcessState.Waiting => EProcessState.Terminated,
                        EProcessState.Terminated => EProcessState.Terminated,
                        _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
                    },
                    _ => throw new ArgumentOutOfRangeException(nameof(lifecycle), lifecycle, null)
                },
                _ => throw new ArgumentOutOfRangeException(nameof(State), State, null)
            };
        }
        
        public bool ValidatePCBStateTransfer(ProcessControlBlock pcb, EProcessState to)
        {
            var lifecycle = pcb.Process.Lifecycle;
            var from = pcb.State;
            
            return State switch
            {

                EProcessControlState.Ready => lifecycle switch
                {

                    EProcessLifecycle.SelfTerminating => to switch
                    {
                        EProcessState.Created => false,
                        EProcessState.Running => from == EProcessState.Created,
                        EProcessState.Waiting => false,
                        EProcessState.Terminated => from == EProcessState.Running,
                        _ => throw new ArgumentOutOfRangeException(nameof(to), to, null)
                    },
                    EProcessLifecycle.RunThenWait => to switch
                    {
                        EProcessState.Created => false,
                        EProcessState.Running => from is EProcessState.Waiting,
                        EProcessState.Waiting => from is EProcessState.Running,
                        EProcessState.Terminated => from is EProcessState.Running or EProcessState.Waiting,
                        _ => throw new ArgumentOutOfRangeException(nameof(to), to, null)
                    },
                    EProcessLifecycle.RequiresControl => to switch
                    {
                        EProcessState.Created => false,
                        EProcessState.Running => from is EProcessState.Created or EProcessState.Waiting,
                        EProcessState.Waiting => from is EProcessState.Running,
                        EProcessState.Terminated => from is EProcessState.Running or EProcessState.Waiting,
                        _ => throw new ArgumentOutOfRangeException(nameof(to), to, null)
                    },
                    _ => throw new ArgumentOutOfRangeException(nameof(lifecycle), lifecycle, null)
                },
                EProcessControlState.Waiting => lifecycle switch
                {
                    EProcessLifecycle.SelfTerminating => to switch
                    {
                        EProcessState.Created => false,
                        EProcessState.Running => false,
                        EProcessState.Waiting => false,
                        EProcessState.Terminated => true,
                        _ => throw new ArgumentOutOfRangeException(nameof(to), to, null)
                    },
                    EProcessLifecycle.RunThenWait => to switch
                    {
                        EProcessState.Created => false,
                        EProcessState.Running => false,
                        EProcessState.Waiting => false,
                        EProcessState.Terminated => true,
                        _ => throw new ArgumentOutOfRangeException(nameof(to), to, null)
                    },
                    EProcessLifecycle.RequiresControl => to switch
                    {
                        EProcessState.Created => false,
                        EProcessState.Running => false,
                        EProcessState.Waiting => false,
                        EProcessState.Terminated => true,
                        _ => throw new ArgumentOutOfRangeException(nameof(to), to, null)
                    },
                    _ => throw new ArgumentOutOfRangeException(nameof(lifecycle), lifecycle, null)
                },
                EProcessControlState.Closed => lifecycle switch
                {
                    EProcessLifecycle.SelfTerminating => to switch
                    {
                        EProcessState.Created => false,
                        EProcessState.Running => from == EProcessState.Created,
                        EProcessState.Waiting => false,
                        EProcessState.Terminated => from == EProcessState.Running,
                        _ => throw new ArgumentOutOfRangeException(nameof(to), to, null)
                    },
                    EProcessLifecycle.RunThenWait => to switch
                    {
                        EProcessState.Created => false,
                        EProcessState.Running => from is EProcessState.Waiting,
                        EProcessState.Waiting => from is EProcessState.Running,
                        EProcessState.Terminated => from is EProcessState.Running or EProcessState.Waiting,
                        _ => throw new ArgumentOutOfRangeException(nameof(to), to, null)
                    },
                    EProcessLifecycle.RequiresControl => to switch
                    {
                        EProcessState.Created => false,
                        EProcessState.Running => from is EProcessState.Created or EProcessState.Waiting,
                        EProcessState.Waiting => from is EProcessState.Running,
                        EProcessState.Terminated => from is EProcessState.Running or EProcessState.Waiting,
                        _ => throw new ArgumentOutOfRangeException(nameof(to), to, null)
                    },
                    _ => throw new ArgumentOutOfRangeException(nameof(lifecycle), lifecycle, null)
                },
                EProcessControlState.ClosedWaiting => lifecycle switch
                {
                    EProcessLifecycle.SelfTerminating => to switch
                    {
                        EProcessState.Created => false,
                        EProcessState.Running => false,
                        EProcessState.Waiting => false,
                        EProcessState.Terminated => true,
                        _ => throw new ArgumentOutOfRangeException(nameof(to), to, null)
                    },
                    EProcessLifecycle.RunThenWait => to switch
                    {
                        EProcessState.Created => false,
                        EProcessState.Running => false,
                        EProcessState.Waiting => false,
                        EProcessState.Terminated => true,
                        _ => throw new ArgumentOutOfRangeException(nameof(to), to, null)
                    },
                    EProcessLifecycle.RequiresControl => to switch
                    {
                        EProcessState.Created => false,
                        EProcessState.Running => false,
                        EProcessState.Waiting => false,
                        EProcessState.Terminated => true,
                        _ => throw new ArgumentOutOfRangeException(nameof(to), to, null)
                    },
                    _ => throw new ArgumentOutOfRangeException(nameof(lifecycle), lifecycle, null)
                },
                EProcessControlState.Terminated => lifecycle switch
                {
                    EProcessLifecycle.SelfTerminating => to switch
                    {
                        EProcessState.Created => false,
                        EProcessState.Running => false,
                        EProcessState.Waiting => false,
                        EProcessState.Terminated => false,
                        _ => throw new ArgumentOutOfRangeException(nameof(to), to, null)
                    },
                    EProcessLifecycle.RunThenWait => to switch
                    {
                        EProcessState.Created => false,
                        EProcessState.Running => false,
                        EProcessState.Waiting => false,
                        EProcessState.Terminated => false,
                        _ => throw new ArgumentOutOfRangeException(nameof(to), to, null)
                    },
                    EProcessLifecycle.RequiresControl => to switch
                    {
                        EProcessState.Created => false,
                        EProcessState.Running => false,
                        EProcessState.Waiting => false,
                        EProcessState.Terminated => false,
                        _ => throw new ArgumentOutOfRangeException(nameof(to), to, null)
                    },
                    _ => throw new ArgumentOutOfRangeException(nameof(lifecycle), lifecycle, null)
                },
                EProcessControlState.TerminatedImmediately => lifecycle switch
                {
                    EProcessLifecycle.SelfTerminating => to switch
                    {
                        EProcessState.Created => false,
                        EProcessState.Running => false,
                        EProcessState.Waiting => false,
                        EProcessState.Terminated => false,
                        _ => throw new ArgumentOutOfRangeException(nameof(to), to, null)
                    },
                    EProcessLifecycle.RunThenWait => to switch
                    {
                        EProcessState.Created => false,
                        EProcessState.Running => false,
                        EProcessState.Waiting => false,
                        EProcessState.Terminated => false,
                        _ => throw new ArgumentOutOfRangeException(nameof(to), to, null)
                    },
                    EProcessLifecycle.RequiresControl => to switch
                    {
                        EProcessState.Created => false,
                        EProcessState.Running => false,
                        EProcessState.Waiting => false,
                        EProcessState.Terminated => false,
                        _ => throw new ArgumentOutOfRangeException(nameof(to), to, null)
                    },
                    _ => throw new ArgumentOutOfRangeException(nameof(lifecycle), lifecycle, null)
                },
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
        #endregion
        
        private void OnDestroy()
        {
            TerminateAllImmediately();
        }
        
        public bool HandlerValidateAgainst(IGameplayProcessHandler handler)
        {
            return (ProcessControl)handler == this;
        }

        public bool HandlerProcessIsSubscribed(ProcessRelay relay)
        {
            return true;
        }

        public void HandlerSubscribeProcess(ProcessRelay relay)
        {
            // Doesn't need to do anything!
        }
        public bool HandlerVoidProcess(int processIndex)
        {
            // Doesn't need to do anything!
            return true;
        }
    }

    public enum EProcessControlState
    {
        Ready,  // Behave as normal
        Waiting,  // Accept register/unregister requests but don't run processes
        Closed,  // Don't accept any requests but run active processes
        ClosedWaiting,  // Don't accept any requests and don't run active processes
        Terminated,  // Don't accept any requests and terminate all active processes
        TerminatedImmediately  // Same as Terminated but don't let running processes finish
    }
}
