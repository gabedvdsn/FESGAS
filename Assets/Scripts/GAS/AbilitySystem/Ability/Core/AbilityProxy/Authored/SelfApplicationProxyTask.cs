using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(fileName = "New Self Application Proxy Task", menuName = "FESGAS/Ability/Task/Self Application")]
    public class SelfApplicationProxyTask : AbstractAbilityProxyTaskScriptableObject
    {
        public List<GameplayEffectScriptableObject> Effects;
        public int BetweenEffectPauseMilliseconds;
        
        public override async UniTask Activate(AbilitySpec spec, Vector3 position, CancellationToken token)
        {
            await UniTask.CompletedTask;
        }
        
        public override async UniTask Activate(AbilitySpec spec, GASComponent target, CancellationToken token)
        {
            Debug.Log($"SELF APP");
            foreach (GameplayEffectScriptableObject effect in Effects)
            {
                target.ApplyGameplayEffect(spec, effect);
                await UniTask.Delay(BetweenEffectPauseMilliseconds, cancellationToken: token);
            }
            
            await UniTask.CompletedTask;
        }
    }
}
