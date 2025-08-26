using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(menuName = "FESGAS/Magnitude Modifier/Simple", fileName = "MM_Simple_")]
    public class SimpleMagnitudeModifierScriptableObject : AbstractMagnitudeModifierScriptableObject
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
    }
}
