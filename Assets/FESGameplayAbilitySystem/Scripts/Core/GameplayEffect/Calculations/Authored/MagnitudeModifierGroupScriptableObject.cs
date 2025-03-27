using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(fileName = "MMGroup_", menuName = "FESGAS/Magnitude Modifier/Group")]
    public class MagnitudeModifierGroupScriptableObject : AbstractMagnitudeModifierScriptableObject
    {
        public List<MagnitudeModifierGroupMember> Calculations;
        public EValueCollisionPolicy OverrideMemberCollisionPolicy;
        
        public override void Initialize(GameplayEffectSpec spec)
        {
            foreach (MagnitudeModifierGroupMember member in Calculations)
            {
                member.Calculation.Initialize(spec);
            }
        }
        public override float Evaluate(GameplayEffectSpec spec)
        {
            if (Calculations.Any(m => m.RelativeOperation == ECalculationOperation.Override))
            {
                return OverrideMemberCollisionPolicy switch
                {
                    EValueCollisionPolicy.UseMaximum => Calculations.Where(m => m.RelativeOperation == ECalculationOperation.Override)
                        .Max(m => m.Calculation.Evaluate(spec)),
                    EValueCollisionPolicy.UseMinimum => Calculations.Where(m => m.RelativeOperation == ECalculationOperation.Override)
                        .Min(m => m.Calculation.Evaluate(spec)),
                    EValueCollisionPolicy.UseAverage => Calculations.Where(m => m.RelativeOperation == ECalculationOperation.Override)
                        .Average(m => m.Calculation.Evaluate(spec)),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
            
            float value = Calculations.Where(m => m.RelativeOperation == ECalculationOperation.Add).Sum(member => member.Calculation.Evaluate(spec));
            if (value == 0f) value = 1f;
            return Calculations.Where(m => m.RelativeOperation == ECalculationOperation.Multiply).Aggregate(value, (current, member) => current * member.Calculation.Evaluate(spec));
        }

        [Serializable]
        public struct MagnitudeModifierGroupMember
        {
            public AbstractMagnitudeModifierScriptableObject Calculation;
            public ECalculationOperation RelativeOperation;
        }
    }
}
