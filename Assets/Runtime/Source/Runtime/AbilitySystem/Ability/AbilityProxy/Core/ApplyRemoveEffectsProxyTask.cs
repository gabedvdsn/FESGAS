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

        public override void Prepare(AbilityDataPacket data)
        {
            if (!data.TryGetFirst(ITag.Get(TagChannels.PAYLOAD_TARGET), out ITarget target))
            {
                return;
            }

            var gas = target.AsGAS();
            
            foreach (GameplayEffectScriptableObject effect in Effects) target.ApplyGameplayEffect(gas.GenerateEffectSpec(data.Spec, effect));
        }

        public override async UniTask Activate(AbilityDataPacket data, CancellationToken token)
        {
            await UniTask.CompletedTask;
        }
        
        public override void Clean(AbilityDataPacket data)
        {
            if (!data.TryGet(ITag.Get(TagChannels.PAYLOAD_TARGET), EProxyDataValueTarget.Primary, out ITarget target))
            {
                return;
            }
            var gas = target.AsGAS();
            foreach (GameplayEffectScriptableObject effect in Effects) gas.RemoveGameplayEffect(effect);
        }
    }
}
