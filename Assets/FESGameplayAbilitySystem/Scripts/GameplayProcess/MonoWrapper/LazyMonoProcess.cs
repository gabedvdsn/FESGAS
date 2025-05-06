using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    /// <summary>
    /// The Lazy process is a process that does absolutely nothing and runs until termination
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
    }
}
