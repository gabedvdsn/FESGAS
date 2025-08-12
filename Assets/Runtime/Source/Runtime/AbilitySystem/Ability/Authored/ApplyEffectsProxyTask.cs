using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(fileName = "PT_ApplyEffects_", menuName = "FESGAS/Ability/Task/Apply Effects")]
    public class ApplyEffectsProxyTask : AbstractAbilityProxyTaskScriptableObject
    {
        public List<GameplayEffectScriptableObject> Effects;
        public int BetweenApplicationDelayMilliseconds;
        
        public override async UniTask Activate(AbilityDataPacket data, CancellationToken token)
        {
            if (!TryGetTarget(data, out var target, EProxyDataValueTarget.Primary))
            {
                
            }
            
            foreach (GameplayEffectScriptableObject effect in Effects)
            {
                target.ApplyGameplayEffect(target.GenerateEffectSpec(data.Spec, effect));
                await UniTask.Delay(BetweenApplicationDelayMilliseconds, cancellationToken: token);
            }
        }
        public override bool IsCriticalSection => false;
    }
}
