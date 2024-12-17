using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(menuName = "FESGAS/Gameplay Effect Calculation/Level", fileName = "New Level Gameplay Effect Calculation")]
    public class LevelGameplayEffectCalculationScriptableObject : AbstractGameplayEffectCalculationScriptableObject
    {
        public AnimationCurve Scaling;

        public override void Initialize(GameplayEffectSpec spec)
        {
            
        }
        
        public override float Evaluate(AbilitySystemComponent target, AbilitySystemComponent source, GameplayEffectSpec gameplayEffect)
        {
            return Scaling.Evaluate(gameplayEffect.Level);
        }
    }
}
