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
        
        protected ProcessDataPacket data;
        protected bool processActive;
        
        public void SendProcessData(ProcessDataPacket processData)
        {
            data = processData;
        }

        public virtual void WhenInitialize(ProcessRelay relay)
        {
            if (data.TryGetPayload<Vector3>(InitialPositionTarget, GameRoot.PositionTag, InitialPositionValue, out var pos))
            {
                transform.position = pos;
            }
            else if (data.TryGetPayload<GASComponent>(InitialPositionTarget, GameRoot.GASTag, InitialPositionValue, out var gasPos))
            {
                transform.position = gasPos.transform.position;
            }
            
            if (data.TryGetPayload<Quaternion>(InitialRotationTarget, GameRoot.RotationTag, InitialRotationValue, out var rot))
            {
                transform.rotation = rot;
            }
            else if (data.TryGetPayload<GASComponent>(InitialRotationTarget, GameRoot.GASTag, InitialRotationValue, out var gasRot))
            {
                transform.rotation = gasRot.transform.rotation;
            }
            
            if (data.TryGetPayload<Transform>(ParentTransformTarget, GameRoot.TransformTag, ParentTransformValue, out var pt))
            {
                transform.SetParent(pt);
            }
            else transform.SetParent(GameRoot.Instance.transform);
        }
        
        public abstract void WhenUpdate(ProcessRelay relay);

        public virtual void WhenWait(ProcessRelay relay)
        {
            processActive = false;
        }

        public virtual void WhenTerminate(ProcessRelay relay)
        {
            processActive = false;
        }
        
        public abstract UniTask RunProcess(ProcessRelay relay, CancellationToken token);

        public int StepPriority => ProcessStepPriority;
        public EProcessUpdateTiming StepTiming => ProcessTiming;
        public EProcessLifecycle Lifecycle => ProcessLifecycle;
    }
}
