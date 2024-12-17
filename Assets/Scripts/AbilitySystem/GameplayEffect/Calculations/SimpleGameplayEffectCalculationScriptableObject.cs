using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(menuName = "FESGAS/Gameplay Effect Calculation/Level", fileName = "New Simple Gameplay Effect Calculation")]
    public class SimpleGameplayEffectCalculationScriptableObject : AbstractGameplayEffectCalculationScriptableObject
    {
        public AnimationCurve Scaling;

        public override void Initialize(GameplayEffectSpec spec)
        {
            
        }
        
        public override float Evaluate(GameplayEffectSpec spec)
        {
            return Scaling.Evaluate(spec.Level);
        }
    }
}
