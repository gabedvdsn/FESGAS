using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public abstract class AbstractEffectingMonoProcess : AbstractDerivedMonoProcess
    {
        [Header("Effects")]
        
        public List<GameplayEffectScriptableObject> Effects;

        protected void ApplyEffects(GASComponentBase target)
        {
            foreach (var effect in Effects) target.ApplyGameplayEffect(Derivation, effect);
        }
    }
}
