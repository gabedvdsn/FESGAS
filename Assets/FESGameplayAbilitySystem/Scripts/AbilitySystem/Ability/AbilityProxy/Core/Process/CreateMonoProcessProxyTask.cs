using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(fileName = "PT_CreateMonoProcess_", menuName = "FESGAS/Ability/Task/Create Mono Process")]
    public class CreateMonoProcessProxyTask : AbstractCreateProcessProxyTask
    {
        [Header("Create Mono Processes")]
        
        public List<MonoProcessPacket> MonoProcesses;
        
        public override async UniTask Activate(ProxyDataPacket data, CancellationToken token)
        {
            foreach (var packet in MonoProcesses)
            {
                ProcessControl.Instance.RegisterMonoProcess(packet, data, UseDefaultParameters ? GameRoot.Instance.DefaultDataParameters : Parameters, out _);
            }
            
            await UniTask.CompletedTask;
        }
    }

    [Serializable]
    public struct MonoProcessPacket
    {
        public AbstractMonoProcess MonoProcess;

        public MonoProcessPacket(AbstractMonoProcess monoProcess) : this()
        {
            MonoProcess = monoProcess;
        }

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
