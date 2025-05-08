using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public abstract class AbstractEffectingMonoProcess : LazyMonoProcess
    {
        [Header("Effects")]
        
        public List<GameplayEffectScriptableObject> Effects;

        protected void ApplyEffects(GASComponentBase target)
        {
            if (!data.TryGetPayload(GameRoot.DerivationTag, ESourceTargetData.Data, EProxyDataValueTarget.Primary, out IEffectDerivation derivation))
            {
                derivation = GameRoot.Instance;
            }

            foreach (var effect in Effects) target.ApplyGameplayEffect(derivation, effect);
        }
    }
}
