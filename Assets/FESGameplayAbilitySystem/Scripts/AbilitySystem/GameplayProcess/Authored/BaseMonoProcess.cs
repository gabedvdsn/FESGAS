using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public class BaseMonoProcess : AbstractMonoProcess
    {
        
        public override void WhenInitialize(ProcessRelay relay)
        {
            
        }
        public override void WhenUpdate(ProcessRelay relay)
        {
            transform.Rotate(Vector3.up * (relay.Runtime * .25f));
        }
        public override void WhenWait(ProcessRelay relay)
        {
            
        }
        public override void WhenTerminate(ProcessRelay relay)
        {
            Destroy(gameObject);
        }
        public override async UniTask RunProcess(ProcessRelay relay, CancellationToken token)
        {
            await UniTask.Delay(5000, cancellationToken: token);
        }
    }
}
