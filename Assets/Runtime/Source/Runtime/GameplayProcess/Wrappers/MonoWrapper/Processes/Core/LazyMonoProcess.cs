using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    /// <summary>
    /// The Lazy process is a process (typically self-terminating) that runs until Waited or Terminated.
    /// </summary>
    public class LazyMonoProcess : AbstractMonoProcess
    {
        public override void WhenUpdate(ProcessRelay relay)
        {
            
        }

        public override async UniTask RunProcess(ProcessRelay relay, CancellationToken token)
        {
            processActive = true;
            await UniTask.WaitWhile(() => processActive, cancellationToken: token);
        }

        private void OnDestroy()
        {
            UnityEngine.Debug.Log($"Destoy {name}");
        }
    }
}
