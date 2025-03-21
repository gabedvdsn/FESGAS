using System;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(fileName = "MM_AttributeBacked_", menuName = "FESGAS/Magnitude Modifier/Attribute Backed", order = 0)]
    public class AttributeBackedMagnitudeModifier : AbstractMagnitudeModifierScriptableObject
    {
        public AttributedBackedScalingPolicy ScalingPolicy;
        public AnimationCurve Scaling;
        
        [Space]
        
        public AttributeScriptableObject CaptureAttribute;
        public ESourceTarget CaptureFrom;
        public ECaptureAttributeWhen CaptureWhen;
        
        public override void Initialize(GameplayEffectSpec spec)
        {
            if (CaptureWhen != ECaptureAttributeWhen.OnCreation) return;
            
            switch (CaptureFrom)
            {
                case ESourceTarget.Source:
                    if (!spec.Source.AttributeSystem.TryGetAttributeValue(CaptureAttribute, out AttributeValue sourceAttributeValue)) break;
                    spec.SourceCapturedAttributes[this] = sourceAttributeValue;
                    break;
                case ESourceTarget.Target:
                    if (!spec.Target.AttributeSystem.TryGetAttributeValue(CaptureAttribute, out AttributeValue targetAttributeValue)) break;
                    spec.SourceCapturedAttributes[this] = targetAttributeValue;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public override float Evaluate(GameplayEffectSpec spec)
        {
            if (CaptureWhen == ECaptureAttributeWhen.OnCreation)
            {
                return ScalingPolicy switch
                {
                    AttributedBackedScalingPolicy.EvaluateCurrentValue => Scaling.Evaluate(spec.SourceCapturedAttributes[this].GetValueOrDefault().CurrentValue),
                    AttributedBackedScalingPolicy.EvaluateBaseValue => Scaling.Evaluate(spec.SourceCapturedAttributes[this].GetValueOrDefault().BaseValue),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            if (CaptureFrom == ESourceTarget.Source)
            {
                if (!spec.Source.AttributeSystem.TryGetAttributeValue(CaptureAttribute, out AttributeValue attributeValue)) return 0f;
                return ScalingPolicy switch
                {

                    AttributedBackedScalingPolicy.EvaluateCurrentValue => Scaling.Evaluate(attributeValue.CurrentValue),
                    AttributedBackedScalingPolicy.EvaluateBaseValue => Scaling.Evaluate(attributeValue.BaseValue),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
            else
            {
                if (!spec.Target.AttributeSystem.TryGetAttributeValue(CaptureAttribute, out AttributeValue attributeValue)) return 0f;
                return ScalingPolicy switch
                {

                    AttributedBackedScalingPolicy.EvaluateCurrentValue => Scaling.Evaluate(attributeValue.CurrentValue),
                    AttributedBackedScalingPolicy.EvaluateBaseValue => Scaling.Evaluate(attributeValue.BaseValue),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }
    }

    public enum AttributedBackedScalingPolicy
    {
        EvaluateCurrentValue,
        EvaluateBaseValue
    }
    
    public enum ESourceTarget
    {
        Target,
        Source
    }

    public enum ESourceTargetBoth
    {
        Target,
        Source,
        Both
    }

    public enum ECaptureAttributeWhen
    {
        OnCreation,
        OnApplication
    }
}
