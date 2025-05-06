using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(fileName = "PT_ApplyRemoveEffects_", menuName = "FESGAS/Ability/Task/Apply->Remove")]
    public class ApplyRemoveEffectsProxyTask : AbstractAbilityProxyTaskScriptableObject
    {
        public List<GameplayEffectScriptableObject> Effects;

        public override UniTask Prepare(ProxyDataPacket data)
        {
            if (!data.TryGetTarget(GameRoot.GASTag, EProxyDataValueTarget.Primary, out GASComponentBase target))
            {
                return UniTask.CompletedTask;
            }
            foreach (GameplayEffectScriptableObject effect in Effects) target.ApplyGameplayEffect(target.GenerateEffectSpec(data.Spec, effect));
            return base.Prepare(data);
        }

        public override async UniTask Activate(ProxyDataPacket data, CancellationToken token)
        {
            await UniTask.CompletedTask;
        }
        
        public override UniTask Clean(ProxyDataPacket data)
        {
            if (!data.TryGetTarget(GameRoot.GASTag, EProxyDataValueTarget.Primary, out GASComponentBase target))
            {
                return UniTask.CompletedTask;
            }
            foreach (GameplayEffectScriptableObject effect in Effects) target.RemoveGameplayEffect(effect);
            return base.Prepare(data);
        }
    }
}
