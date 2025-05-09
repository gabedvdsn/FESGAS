using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem.Demo
{
    public class TestMonoProcess : AbstractMonoProcess
    {
        public override void WhenUpdate(ProcessRelay relay)
        {
            transform.Rotate(Vector3.up * (relay.UpdateTime * .25f));
        }
        public override void WhenTerminate(ProcessRelay relay)
        {
            Destroy(gameObject);
        }
        public override async UniTask RunProcess(ProcessRelay relay, CancellationToken token)
        {
            await UniTask.Delay(relay.RemainingRuntime(5000), cancellationToken: token);
        }
    }
}
