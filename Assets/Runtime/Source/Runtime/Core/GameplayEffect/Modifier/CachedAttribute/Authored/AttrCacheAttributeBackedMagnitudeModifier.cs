using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public class AttrCacheAttributeBackedMagnitudeModifier : AbstractCachedAttributeMagnitudeModifier
    {
        public AttributeScriptableObject CaptureAttribute;
        public AnimationCurve Scaling;
        public EEffectImpactTargetLimited ScalingPolicy;
        
        public override void Initialize(IAttributeImpactDerivation spec)
        {
            Gasify.Modifier.Init_AttributeBacked(this, CaptureAttribute, ECaptureAttributeWhen.OnApplication, ESourceTarget.Source, spec);
        }
        public override float Evaluate(IAttributeImpactDerivation spec)
        {
            return Gasify.Modifier.Eval_AttributeBacked(
                this, 
                Scaling, 
                ScalingPolicy, 
                CaptureAttribute, 
                ECaptureAttributeWhen.OnApplication, ESourceTarget.Source, 
                spec);
        }
        public override void Regulate(IAttribute attribute, AttributeModificationRule rules)
        {
            rules.RegisterRelation(CaptureAttribute, attribute);
        }
    }
}
