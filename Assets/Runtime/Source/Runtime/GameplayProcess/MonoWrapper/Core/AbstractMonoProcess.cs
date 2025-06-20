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
        
        [Header("Parameter Usage")]
        
        public ESourceTargetData InitialPositionTarget = ESourceTargetData.Source;
        public EProxyDataValueTarget InitialPositionValue = EProxyDataValueTarget.Primary;
        
        [Space(5)]
        
        public ESourceTargetData InitialRotationTarget = ESourceTargetData.Source;
        public EProxyDataValueTarget InitialRotationValue = EProxyDataValueTarget.Primary;
        
        [Space(5)]
        
        public ESourceTargetData ParentTransformTarget = ESourceTargetData.Data;
        public EProxyDataValueTarget ParentTransformValue = EProxyDataValueTarget.Primary;
        
        protected ProcessDataPacket regData;
        protected bool processActive;

        private bool _initialized;
        public bool IsInitialized => _initialized;
        
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
            _initialized = true;
            
            if (regData.TryGetPayload(GameRoot.GenericTag, ESourceTargetData.Data, EProxyDataValueTarget.Primary, out GameplayTagScriptableObject affiliation))
            {
                
            }
            
            if (regData.TryGetPayload<Vector3>(GameRoot.PositionTag, InitialPositionTarget, InitialPositionValue, out var pos))
            {
                transform.position = pos;
            }
            else if (regData.TryGetPayload<GASComponent>(GameRoot.GASTag, InitialPositionTarget, InitialPositionValue, out var gasPos))
            {
                transform.position = gasPos.transform.position;
            }
            
            if (regData.TryGetPayload<Quaternion>(GameRoot.RotationTag, InitialRotationTarget, InitialRotationValue, out var rot))
            {
                transform.rotation = rot;
            }
            else if (regData.TryGetPayload<GASComponent>(GameRoot.GASTag, InitialRotationTarget, InitialRotationValue, out var gasRot))
            {
                transform.rotation = gasRot.transform.rotation;
            }
            
            if (regData.TryGetPayload<Transform>(GameRoot.TransformTag, ParentTransformTarget, ParentTransformValue, out var pt))
            {
                transform.SetParent(pt);
            }
            else transform.SetParent(GameRoot.Instance.transform);
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
        
        public void WhenTerminateSafe(ProcessRelay relay)
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

        public int StepPriority => ProcessStepPriority;
        public EProcessUpdateTiming StepTiming => ProcessTiming;
        public EProcessLifecycle Lifecycle => ProcessLifecycle;
    }
}
