using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public abstract class AbstractMonoProcess : MonoBehaviour
    {
        [Header("Mono Gameplay Process")] 
        
        public EProcessLifecycle ProcessLifecycle;
        public EProcessUpdateTiming ProcessTiming;
        public EProcessStepPriorityMethod PriorityMethod = EProcessStepPriorityMethod.Manual;
        public int ProcessStepPriority;
        
        [Space(5)]
        
        [Tooltip("Uses Object.Instantiate when null")]
        public AbstractMonoProcessInstantiatorScriptableObject Instantiator;
        
        protected ProcessDataPacket regData;
        protected bool processActive;

        private bool _initialized;
        public bool IsInitialized => _initialized;
        public ProcessRelay Relay;
        
        public void SendProcessData(ProcessDataPacket processData)
        {
            regData = processData;
        }

        /// <summary>
        /// Called via ProcessControl after the process is Created
        /// </summary>
        /// <param name="relay">Process Relay</param>
        public virtual void WhenInitialize(ProcessRelay relay)
        {
            Debug.Log($"\t\t{this.ToString()} initialized");
            _initialized = true;
            Relay = relay;
            
            // Transform
            if (regData.TryGet<Transform>(ITag.Get(TagChannels.PAYLOAD_TRANSFORM), EProxyDataValueTarget.Primary, out var pt))
            {
                transform.SetParent(pt);
            }
            
            // Position
            if (regData.TryGet<Vector3>(ITag.Get(TagChannels.PAYLOAD_POSITION), EProxyDataValueTarget.Primary, out var pos))
            {
                transform.position = pos;
            }
            else if (regData.TryGet<GASComponent>(ITag.Get(TagChannels.PAYLOAD_POSITION), EProxyDataValueTarget.Primary, out var gasPos))
            {
                transform.position = gasPos.transform.position;
            }
            else if (regData.TryGet<Transform>(ITag.Get(TagChannels.PAYLOAD_POSITION), EProxyDataValueTarget.Primary, out var tPos))
            {
                transform.position = tPos.position;
            }
            
            // Rotation
            if (regData.TryGet<Quaternion>(ITag.Get(TagChannels.PAYLOAD_ROTATION), EProxyDataValueTarget.Primary, out var rot))
            {
                transform.rotation = rot;
            }
            else if (regData.TryGet<GASComponent>(ITag.Get(TagChannels.PAYLOAD_ROTATION), EProxyDataValueTarget.Primary, out var gasRot))
            {
                transform.rotation = gasRot.transform.rotation;
            }
            else if (regData.TryGet<Transform>(ITag.Get(TagChannels.PAYLOAD_ROTATION), EProxyDataValueTarget.Primary, out var tRot))
            {
                transform.rotation = tRot.rotation;
            }
            
        }
        
        /// <summary>
        /// Called via Step in ProcessControl as determined by the process's StepUpdateTiming
        /// </summary>
        /// <param name="relay">Process Relay</param>
        public abstract void WhenUpdate(ProcessRelay relay);

        /// <summary>
        /// Called via ProcessControl when the process is set to Waiting
        /// </summary>
        /// <param name="relay">Process Relay</param>
        public virtual void WhenWait(ProcessRelay relay)
        {
            processActive = false;
        }

        /// <summary>
        /// Called via ProcessControl when the process is set to Terminated
        /// </summary>
        /// <param name="relay">Process Relay</param>
        public virtual void WhenTerminate(ProcessRelay relay)
        {
            processActive = false;
        }
        
        /// <summary>
        /// Called via ProcessControl when the process is set to Running
        /// </summary>
        /// <param name="relay">Process Relay</param>
        /// <param name="token">Cancellation token</param>
        /// <returns></returns>
        public abstract UniTask RunProcess(ProcessRelay relay, CancellationToken token);

        public override string ToString()
        {
            return name;
        }
    }

    public enum EProcessStepPriorityMethod
    {
        Manual,
        First,
        Last
    }
}
