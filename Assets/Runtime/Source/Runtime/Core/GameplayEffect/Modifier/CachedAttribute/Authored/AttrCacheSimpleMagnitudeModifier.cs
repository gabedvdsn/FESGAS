using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public class AttrCacheSimpleMagnitudeModifier : AbstractCachedAttributeMagnitudeModifier
    {
        public AnimationCurve Scaling;

        public override void Initialize(IAttributeImpactDerivation spec)
        {
            Gasify.Modifier.Init_Simple(Scaling);
        }
        public override float Evaluate(IAttributeImpactDerivation spec)
        {
            return Gasify.Modifier.Eval_Simple(Scaling, spec);
        }

        public override void Regulate(IAttribute attribute, AttributeModificationRule rules)
        {
            // Doesn't do anything
        }
    }
}
