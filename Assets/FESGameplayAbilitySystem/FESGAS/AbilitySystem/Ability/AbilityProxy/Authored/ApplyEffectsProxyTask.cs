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
        
        public override async UniTask Activate(ProxyDataPacket data, CancellationToken token)
        {
            if (!data.TryGetTarget(GameRoot.GASTag, EProxyDataValueTarget.Primary, out GASComponentBase target))
            {
                await UniTask.CompletedTask;
                return;
            }
            
            foreach (GameplayEffectScriptableObject effect in Effects)
            {
                target.ApplyGameplayEffect(target.GenerateEffectSpec(data.Spec, effect));
                await UniTask.Delay(BetweenApplicationDelayMilliseconds, cancellationToken: token);
            }
        }
    }
}
