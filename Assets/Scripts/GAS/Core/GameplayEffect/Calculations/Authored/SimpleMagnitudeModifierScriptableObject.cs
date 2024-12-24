using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(menuName = "FESGAS/Magnitude Modifier/Simple", fileName = "MM_Simple_")]
    public class SimpleMagnitudeModifierScriptableObject : AbstractMagnitudeModifierScriptableObject
    {
        public AnimationCurve Scaling;

        public override void Initialize(GameplayEffectSpec spec)
        {
            
        }
        
        public override float Evaluate(GameplayEffectSpec spec)
        {
            return Scaling.Evaluate(spec.RelativeLevel);
        }
    }
}
