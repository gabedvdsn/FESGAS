using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Networking.PlayerConnection;
using UnityEngine;
using UnityEngine.Serialization;
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
        
        public MonoProcessPacket TestMonoST;  // Testing purposes only, won't be included in shipped version
        public MonoProcessPacket TestMonoRTW;  // Testing purposes only, won't be included in shipped version
        public MonoProcessPacket TestMonoRC;  // Testing purposes only, won't be included in shipped version

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
                var process = PrepareMonoProcess(TestMonoST, null, null);
                Instance.Register(process, null, out _);
            }
            
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                //var process = new TestClassProcess();
                var process = PrepareMonoProcess(TestMonoRTW, null, null);
                Instance.Register(process, null, out _);
            }
            
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                //var process = new TestClassProcess();
                var process = PrepareMonoProcess(TestMonoRC, null, null);
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
            if (State is EProcessControlState.Waiting or EProcessControlState.TerminatedImmediately) return;
            
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
                NextCacheIndex, -1,
                process, handler
            );

            SetProcess(pcb);
            relay = pcb.GetRelay();
            
            return true;
        }

        // Unregister a PCB
        public bool Unregister(ProcessControlBlock pcb)
        {
            if (waiting.Contains(pcb.CacheIndex)) waiting.Remove(pcb.CacheIndex);
            else RemoveFromStepping(pcb);

            pcb.Handler?.HandlerVoidProcess(pcb.CacheIndex);
            
            return active.Remove(pcb.CacheIndex);
        }

        public Dictionary<int, ProcessControlBlock> FetchActiveProcesses()
        {
            return active;
        }

        public MonoWrapperProcess PrepareMonoProcess(MonoProcessPacket packet, MonoProcessParametersScriptableObject parameters, ProxyDataPacket data)
        {
            // Can't use data bc don't know how to grab payload data
            // Default to Identity parameters
            if (parameters is null || data is null)
            {
                return new MonoWrapperProcess(packet.MonoProcess, Vector3.zero, Quaternion.identity);
            }

            MonoWrapperProcess process = new MonoWrapperProcess(packet.MonoProcess);
            
            // Position
            if (data.TryGetPayload<Vector3>(packet.Position, parameters.PositionTag, out var posData) && posData.Valid)
            {
                process.SetPosition(posData.Primary);
            }
            
            // Rotation
            if (data.TryGetPayload<Quaternion>(packet.Rotation, parameters.RotationTag, out var rotData) && rotData.Valid)
            {
                process.SetRotation(rotData.Primary);
            }
            
            // Parent Transform
            if (data.TryGetPayload<Transform>(packet.Rotation, parameters.RotationTag, out var ptData) && ptData.Valid)
            {
                process.SetParentTransform(ptData.Primary);
            }

            return process;
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

        public bool TerminateImmediate(int cacheIndex)
        {
            if (!active.ContainsKey(cacheIndex)) return false;
            
            active[cacheIndex].ForceIntoState(EProcessState.Terminated);

            return true;
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
                active[cacheIndex].ForceIntoState(EProcessState.Terminated);
            }
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
            if (pcb.StepIndex >= 0) return;
            
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
            if (pcb.StepIndex < 0) return;
            
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
            pcb.SetStepIndex(-1);
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
