using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(fileName = "PT_CreateMonoProcess_", menuName = "FESGAS/Ability/Task/Create Mono Process")]
    public class CreateMonoProcessProxyTask : AbstractAbilityProxyTaskScriptableObject
    {
        public List<MonoProcessPacket> MonoProcesses;
        public MonoProcessParametersScriptableObject Parameters;
        
        public override async UniTask Activate(ProxyDataPacket data, CancellationToken token)
        {
            // Can't use data bc don't know how to grab payload data
            if (Parameters is null)
            {
                foreach (var packet in MonoProcesses)
                {
                    ProcessControl.Instance.Register
                    (
                        new MonoWrapperProcess(packet.MonoProcess, Vector3.zero, Quaternion.identity),
                        data.Spec.Owner,
                        out var relay
                    );
                    data.Spec.Owner.HandlerSubscribeProcess(relay);
                }
            }
            else
            {
                foreach (var packet in MonoProcesses)
                {
                    ProcessControl.Instance.Register
                    (
                        ProcessControl.Instance.PrepareMonoProcess(packet, Parameters, data),
                        data.Spec.Owner,
                        out var relay
                    );
                    data.Spec.Owner.HandlerSubscribeProcess(relay);
                }
            }
            
            await UniTask.CompletedTask;
        }
    }

    [Serializable]
    public struct MonoProcessPacket
    {
        public AbstractMonoProcess MonoProcess;

        [Space(5)]
        
        public ESourceTargetData Transform;
        public EProxyDataValueTarget TransformTarget;
        
        [Space(5)]
        
        public ESourceTargetData Position;
        public EProxyDataValueTarget PositionTarget;
        
        [Space(5)]
        
        public ESourceTargetData Rotation;
        public EProxyDataValueTarget RotationTarget;
        
    }
}
