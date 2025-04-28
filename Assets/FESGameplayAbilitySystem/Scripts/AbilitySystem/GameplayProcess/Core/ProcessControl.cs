using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace FESGameplayAbilitySystem
{
    public class ProcessControl : MonoBehaviour
    {
        // Singleton instance
        public static ProcessControl Instance;

        [Header("Process Control")] 
        
        public EProcessControlState StartState = EProcessControlState.Ready;
        public new bool DontDestroyOnLoad = true;
        public AbstractMonoProcessDataScriptableObject TestMono;  // Testing purposes only, won't be included in shipped version

        public EProcessControlState State { get; private set; }

        private Dictionary<int, ProcessControlBlock> active = new();
        private Dictionary<EProcessUpdateTiming, SortedDictionary<int, List<int>>> stepping;
        private HashSet<int> waiting = new();

        private int cacheCounter = 0;
        private int NextCacheIndex => cacheCounter++;

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
            
            // For testing purposes only
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                //var process = new TestClassProcess();
                var process = new MonoWrapperProcess(TestMono, Vector3.zero, Quaternion.identity);
                Instance.Register(process, null, out _);
            }
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
            if (State is EProcessControlState.Waiting or EProcessControlState.Terminated) return;
            
            foreach (var priority in stepping[timing])
            {
                foreach (var cacheIndex in priority.Value) active[cacheIndex].Step();
            }
        }
        
        #endregion

        #region Core
        
        public void SetControlState(EProcessControlState state)
        {
            if (state == State) return;
            
            State = state;

            SetAllProcesses();
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
            Debug.Log($"Registering process {process} ({handler})");
            
            relay = default;
            if (State is EProcessControlState.Closed or EProcessControlState.Terminated) return false;

            var pcb = ProcessControlBlock.Generate(
                NextCacheIndex, -1,
                process, handler,
                GetDefaultCreatedTransitionState(process.Lifecycle)
            );

            SetProcess(pcb);
            relay = pcb.GetRelay();
            
            return true;
        }

        // Unregister a PCB
        public bool Unregister(ProcessControlBlock pcb)
        {
            // The PCB has already taken care of termination logic (and is Termination state)
            Debug.Log($"Unregistering process {pcb.CacheIndex}");
            
            if (waiting.Contains(pcb.CacheIndex)) waiting.Remove(pcb.CacheIndex);
            else RemoveFromStepping(pcb);
            
            return active.Remove(pcb.CacheIndex);
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
            SetProcess(active[cacheIndex]);
            return true;
        }
        
        public bool Wait(int cacheIndex)
        {
            if (!active.ContainsKey(cacheIndex)) return false;
            //MoveFromSteppingToWaiting(active[cacheIndex]);
            active[cacheIndex].Wait();
            return true;
        }

        public bool Pause(int cacheIndex)
        {
            return active.ContainsKey(cacheIndex) && active[cacheIndex].Pause();
        }
        
        public bool Terminate(int cacheIndex)
        {
            if (!active.ContainsKey(cacheIndex)) return false;
            active[cacheIndex].Terminate();
            return true;
        }

        public bool TerminateImmediate(int cacheIndex)
        {
            if (!active.ContainsKey(cacheIndex)) return false;
            active[cacheIndex].TerminateImmediately();
            return Terminate(cacheIndex);
        }
        
        public void TerminateAll()
        {
            List<int> indices = active.Keys.ToList();
            foreach (int cacheIndex in indices) Terminate(cacheIndex);
        }

        public void TerminateAllImmediately()
        {
            List<int> indices = active.Keys.ToList();
            foreach (int cacheIndex in indices)
            {
                active[cacheIndex].TerminateImmediately();
                Terminate(cacheIndex);
            }
        }
        
        #endregion

        #region Process Setting
        
        private void SetProcess(ProcessControlBlock pcb)
        {
            switch (State)
            {
                case EProcessControlState.Ready or EProcessControlState.Closed:
                    // The PCB was created but hasn't been added to the active or waiting caches
                    if (pcb.State == EProcessState.Created)
                    {
                        PrepareCreatedProcess();
                        SetProcess(pcb);
                        return;
                    }
                    SetRunning();
                    break;
                case EProcessControlState.Waiting or EProcessControlState.ClosedWaiting:
                    SetWaiting();
                    break;
            }
            
            return;

            void PrepareCreatedProcess()
            {
                pcb.Initialize();

                waiting.Add(pcb.CacheIndex);
                active[pcb.CacheIndex] = pcb;
            }
            
            // Move the PCB to the active cache
            void SetRunning()
            {
                // The PCB must be in the waiting cache
                if (pcb.State is EProcessState.Waiting) MoveFromWaitingToStepping(pcb);
            }
            
            // Move the PCB to the waiting cache
            void SetWaiting()
            {
                if (pcb.State != EProcessState.Waiting && pcb.State != EProcessState.Terminated) MoveFromSteppingToWaiting(pcb);
            }
        }
        
        private void SetAllProcesses()
        {
            if (State == EProcessControlState.TerminatedImmediately) TerminateAllImmediately();
            else if (State == EProcessControlState.Terminated) TerminateAll();
            else foreach (var process in active.Values) SetProcess(process);
        }

        private void MoveFromWaitingToStepping(ProcessControlBlock pcb)
        {
            RemoveFromWaiting(pcb);
            MoveToStepping(pcb);

            pcb.Run();
        }

        private void MoveFromSteppingToWaiting(ProcessControlBlock pcb, bool block = true)
        {
            pcb.Wait();
        }

        public void ProcessIsWaiting(ProcessControlBlock pcb)
        {
            RemoveFromStepping(pcb);
            MoveToWaiting(pcb);
        }

        private void MoveToStepping(ProcessControlBlock pcb)
        {
            var timing = pcb.Process.StepTiming;
            int priority = pcb.Process.StepPriority;

            if (!stepping[timing].ContainsKey(priority))
            {
                stepping[timing][priority] = new List<int>();
            }
            
            pcb.SetStepIndex(stepping[pcb.Process.StepTiming][priority].Count);
            stepping[pcb.Process.StepTiming][priority].Add(pcb.CacheIndex);
        }
        
        private void RemoveFromStepping(ProcessControlBlock pcb)
        {
            var timing = pcb.Process.StepTiming;
            int priority = pcb.Process.StepPriority;
            int lastIndex = stepping[timing][priority].Count - 1;
            
            if (pcb.StepIndex != lastIndex)
            {
                int lastCacheIndex = stepping[timing][priority][lastIndex];
                stepping[timing][priority][pcb.StepIndex] = lastCacheIndex;
                active[lastCacheIndex].SetStepIndex(pcb.StepIndex);
            }

            stepping[timing][priority].RemoveAt(lastIndex);

            if (stepping[timing][priority].Count == 0) stepping[timing].Remove(priority);
        }

        private void MoveToWaiting(ProcessControlBlock pcb)
        {
            waiting.Add(pcb.CacheIndex);
        }

        private void RemoveFromWaiting(ProcessControlBlock pcb)
        {
            waiting.Remove(pcb.CacheIndex);
        }
        
        #endregion
        
        #region PCB State Transfers

        public static EProcessState GetDefaultCreatedTransitionState(EProcessLifecycle lifecycle)
        {
            return lifecycle switch
            {
                EProcessLifecycle.SelfTerminating => EProcessState.Running,
                EProcessLifecycle.RunThenWait => EProcessState.Running,
                EProcessLifecycle.DependentRunning => EProcessState.Waiting,
                _ => throw new ArgumentOutOfRangeException(nameof(lifecycle), lifecycle, null)
            };
        }
        
        public static bool ValidatePCBStateTransfer(EProcessLifecycle lifecycle, EProcessState from, EProcessState to)
        {
            switch (lifecycle)
            {

                case EProcessLifecycle.SelfTerminating:
                    switch (from)
                    {
                        case EProcessState.Created:
                            return to == EProcessState.Running;
                            break;
                        case EProcessState.Running:
                            
                            break;
                        case EProcessState.Waiting:
                            break;
                        case EProcessState.Terminated:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(from), from, null);
                    }
                    break;
                case EProcessLifecycle.RunThenWait:
                    break;
                case EProcessLifecycle.DependentRunning:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(lifecycle), lifecycle, null);
            }
        } 
        
        #endregion
        
        private void OnDestroy()
        {
            TerminateAllImmediately();
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
