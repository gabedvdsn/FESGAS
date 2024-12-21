using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(fileName = "New Multi Effect Application Proxy Task", menuName = "FESGAS/Ability/Task/Multi Effect Application")]
    public class MultiEffectApplicationProxyTask : AbstractAbilityProxyTaskScriptableObject
    {
        public List<GameplayEffectScriptableObject> Effects;
        public int BetweenEffectPauseMilliseconds;
        
        public override async UniTask Activate(AbilitySpec spec, Vector3 position, CancellationToken token)
        {
            await UniTask.CompletedTask;
        }
        
        public override async UniTask Activate(AbilitySpec spec, GASComponent target, CancellationToken token)
        {
            foreach (GameplayEffectScriptableObject effect in Effects)
            {
                target.ApplyGameplayEffect(spec, effect);
                await UniTask.Delay(BetweenEffectPauseMilliseconds, cancellationToken: token);
            }
            
            await UniTask.CompletedTask;
        }
    }
}
