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
        
        public override void Initialize(GameplayEffectSpec spec)
        {
            foreach (MagnitudeModifierGroupMember member in Calculations)
            {
                member.Calculation.Initialize(spec);
            }
        }
        public override float Evaluate(GameplayEffectSpec spec)
        {
            if (Calculations.Any(m => m.RelativeOperation == CalculationOperation.Override))
            {
                return Calculations.Where(m => m.RelativeOperation == CalculationOperation.Override).Max(m => m.Calculation.Evaluate(spec));
            }
            
            float value = Calculations.Where(m => m.RelativeOperation == CalculationOperation.Add).Sum(member => member.Calculation.Evaluate(spec));
            if (value == 0f) value = 1f;
            return Calculations.Where(m => m.RelativeOperation == CalculationOperation.Multiply).Aggregate(value, (current, member) => current * member.Calculation.Evaluate(spec));
        }

        [Serializable]
        public struct MagnitudeModifierGroupMember
        {
            public AbstractMagnitudeModifierScriptableObject Calculation;
            public CalculationOperation RelativeOperation;
        }
    }
}
