using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public class CreateMonoProcessProxyTask : AbstractCreateProcessProxyTask
    {
        public CreateMonoProcessProxyTask(List<AbstractMonoProcess> monoProcesses)
        {
            MonoProcesses = monoProcesses;
        }

        protected List<AbstractMonoProcess> MonoProcesses;
        
        public override async UniTask Activate(AbilityDataPacket data, CancellationToken token)
        {
            foreach (var process in MonoProcesses)
            {
                ProcessControl.Instance.Register(process, data, out _);
            }
            
            await UniTask.CompletedTask;
        }
        public override bool IsCriticalSection => false;
    }
}
