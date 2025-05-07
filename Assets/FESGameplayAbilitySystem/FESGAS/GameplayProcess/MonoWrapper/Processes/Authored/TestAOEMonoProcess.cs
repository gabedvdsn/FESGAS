using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public class TestAOEMonoProcess : AbstractEffectingMonoProcess
    {
        public override async UniTask RunProcess(ProcessRelay relay, CancellationToken token)
        {
            await UniTask.Delay(5000, cancellationToken: token);
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log(other);
            if (!other.TryGetComponent(out GASComponentBase gas)) return;
            
            ApplyEffects(gas);
        }
    }
}
