using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(fileName = "PT_ApplyEffects_", menuName = "FESGAS/Ability/Task/Apply Effects")]
    public class ApplyEffectsProxyTask : AbstractAbilityProxyTask
    {
        public List<GameplayEffect> Effects;
        public int BetweenApplicationDelayMilliseconds;
        
        public override async UniTask Activate(AbilityDataPacket data, CancellationToken token)
        {
            if (!data.TryGetFirstTarget(out var target))
            {
                return;
            }
            
            foreach (GameplayEffect effect in Effects)
            {
                target.ApplyGameplayEffect(target.GenerateEffectSpec(data.Spec, effect));
                await UniTask.Delay(BetweenApplicationDelayMilliseconds, cancellationToken: token);
            }
        }
        public override bool IsCriticalSection => false;
    }
}
