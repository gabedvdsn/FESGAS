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
            GASComponentBase target = data.Target().Primary;
            foreach (GameplayEffectScriptableObject effect in Effects) target.ApplyGameplayEffect(target.GenerateEffectSpec(data.Spec, effect));
            return base.Prepare(data);
        }

        public override UniTask Activate(ProxyDataPacket data, CancellationToken token)
        {
            return UniTask.CompletedTask;
        }
        
        public override UniTask Clean(ProxyDataPacket data)
        {
            GASComponentBase target = data.Target().Primary;
            foreach (GameplayEffectScriptableObject effect in Effects) target.RemoveGameplayEffect(effect);
            return base.Prepare(data);
        }
    }
}
