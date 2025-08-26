using System;
using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(fileName = "MMGroup_", menuName = "FESGAS/Magnitude Modifier/Group")]
    public class MagnitudeModifierGroupScriptableObject : AbstractMagnitudeModifierScriptableObject
    {
        public List<MagnitudeModifierGroupMember> Calculations;
        public EValueCollisionPolicy OverrideMemberCollisionPolicy;
        
        public override void Initialize(IAttributeImpactDerivation spec)
        {
            Gasify.Modifier.Init_Group(Calculations, spec);
        }
        public override float Evaluate(IAttributeImpactDerivation spec)
        {
            return Gasify.Modifier.Eval_Group(Calculations, OverrideMemberCollisionPolicy, spec);
        }

        [Serializable]
        public struct MagnitudeModifierGroupMember
        {
            public AbstractMagnitudeModifierScriptableObject Calculation;
            public ECalculationOperation RelativeOperation;
        }
    }
}
