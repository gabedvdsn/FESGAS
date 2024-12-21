using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(fileName = "NewApplyRemoveTask", menuName = "FESGAS/Ability/Task/Apply->Remove Effect")]
    public class ApplyRemoveEffectProxyTask : AbstractAbilityProxyTaskScriptableObject
    {
        public GameplayEffectScriptableObject[] Effects;
        
        public override UniTask Prepare(AbilitySpec spec, GASComponent target, CancellationToken token)
        {
            foreach (GameplayEffectScriptableObject effect in Effects)
            {
                target.ApplyGameplayEffect(spec, effect);
            }
            return base.Prepare(spec, target, token);
        }
        public override UniTask Activate(AbilitySpec spec, Vector3 position, CancellationToken token)
        {
            return UniTask.CompletedTask;
        }
        public override UniTask Activate(AbilitySpec spec, GASComponent target, CancellationToken token)
        {
            return UniTask.CompletedTask;
        }

        public override UniTask Clean(AbilitySpec spec, GASComponent target, CancellationToken token)
        {
            foreach (GameplayEffectScriptableObject effect in Effects) target.RemoveGameplayEffect(effect);
            return base.Clean(spec, target, token);
        }

        private void OnValidate()
        {
            ReadOnlyDescription = "Apply the effects once during Prepare() and remove them during Clean()";
        }
    }
}
