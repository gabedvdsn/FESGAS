using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public class TestAOEMonoProcess : LazyMonoProcess
    {
        public override async UniTask RunProcess(ProcessRelay relay, CancellationToken token)
        {
            await UniTask.Delay(5000, cancellationToken: token);
        }
    }
}
