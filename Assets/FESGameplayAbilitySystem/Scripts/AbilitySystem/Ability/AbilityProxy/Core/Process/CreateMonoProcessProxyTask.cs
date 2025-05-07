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
        
        public List<AbstractMonoProcess> MonoProcesses;
        
        public override async UniTask Activate(ProxyDataPacket data, CancellationToken token)
        {
            foreach (var process in MonoProcesses)
            {
                ProcessControl.Instance.Register(process, data, out _);
            }
            
            await UniTask.CompletedTask;
        }
    }
}
