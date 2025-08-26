using System;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(fileName = "MM_AttributeBacked_", menuName = "FESGAS/Magnitude Modifier/Attribute Backed", order = 0)]
    public class AttributeBackedMagnitudeModifier : AbstractMagnitudeModifierScriptableObject
    {
        public AnimationCurve Scaling;
        public EEffectImpactTargetLimited ScalingPolicy;
        
        [Space]
        
        public AttributeScriptableObject CaptureAttribute;
        public ESourceTarget CaptureFrom;
        public ECaptureAttributeWhen CaptureWhen;
        
        public override void Initialize(IAttributeImpactDerivation spec)
        {
            Gasify.Modifier.Init_AttributeBacked(this, CaptureAttribute, CaptureWhen, CaptureFrom, spec);
        }
        
        public override float Evaluate(IAttributeImpactDerivation spec)
        {
            return Gasify.Modifier.Eval_AttributeBacked(
                this, 
                Scaling, 
                ScalingPolicy, 
                CaptureAttribute, 
                CaptureWhen, CaptureFrom, 
                spec);
        }
    }
    
    public enum ESourceTarget
    {
        Target,
        Source
    }
    
    public enum ECaptureAttributeWhen
    {
        OnCreation,
        OnApplication
    }
}
