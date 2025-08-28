using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public class SimpleMagnitudeModifier : AbstractMagnitudeModifier
    {
        public AnimationCurve Scaling;

        public override void Initialize(IAttributeImpactDerivation spec)
        {
            
        }
        
        public override float Evaluate(IAttributeImpactDerivation spec)
        {
            return Scaling.Evaluate(spec.GetEffectDerivation().GetRelativeLevel());
        }
    }
}
