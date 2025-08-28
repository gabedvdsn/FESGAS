using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public abstract class AbstractEffectingMonoProcess : AbstractDerivedMonoProcess
    {
        [Header("Effects")]
        
        public List<GameplayEffect> Effects;

        protected void ApplyEffects(ITarget target)
        {
            foreach (var effect in Effects) target.ApplyGameplayEffect(target.GenerateEffectSpec(Origin, effect));
        }
    }
}
