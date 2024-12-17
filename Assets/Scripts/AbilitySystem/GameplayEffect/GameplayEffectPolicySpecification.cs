using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace FESGameplayAbilitySystem
{
    [Serializable]
    public class GameplayEffectPolicySpecification
    {
        public GameplayEffectDurationPolicy DurationPolicy;
        public bool TickOnApplication;

        [Space] 
        
        public GameplayEffectDurationCalculationPolicy DurationCalculationPolicy;
        public float Duration;
        public AbstractGameplayEffectCalculationScriptableObject DurationCalculation;
        
        [Space]
        
        public GameplayEffectPeriodCalculationPolicy PeriodCalculationPolicy;
        public AbstractGameplayEffectCalculationScriptableObject PeriodCalculation;

        public void ApplyPolicySpecifications(GameplayEffectSpec spec)
        {
            // Apply duration
            if (DurationPolicy == GameplayEffectDurationPolicy.Durational)
            {
                DurationCalculation.Initialize(spec);
                
                switch (DurationCalculationPolicy)
                {
                    case GameplayEffectDurationCalculationPolicy.MagnitudeIsMultiplicative:
                        spec.TotalDuration = Duration * DurationCalculation.Evaluate(spec);
                        break;
                    case GameplayEffectDurationCalculationPolicy.MagnitudeIsAdditive:
                        spec.TotalDuration = Duration + DurationCalculation.Evaluate(spec);
                        break;
                    case GameplayEffectDurationCalculationPolicy.MagnitudeIsDuration:
                        spec.TotalDuration = DurationCalculation.Evaluate(spec);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
                spec.DurationRemaining = spec.TotalDuration;
            }
            
            PeriodCalculation.Initialize(spec);
            
            // Apply period
            switch (PeriodCalculationPolicy)
            {
                case GameplayEffectPeriodCalculationPolicy.MagnitudeIsTicks:
                    spec.PeriodDuration = spec.TotalDuration / PeriodCalculation.Evaluate(spec);
                    break;
                case GameplayEffectPeriodCalculationPolicy.MagnitudeIsPeriod:
                    spec.PeriodDuration = PeriodCalculation.Evaluate(spec);
                    break;
                case GameplayEffectPeriodCalculationPolicy.MagnitudeIsPeriodRatio:
                    spec.PeriodDuration = spec.TotalDuration * PeriodCalculation.Evaluate(spec);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            spec.TimeUntilPeriodTick = TickOnApplication ? 0f : spec.PeriodDuration;
        }
    }
    
    public enum GameplayEffectDurationPolicy
    {
        Instant,
        Infinite,
        Durational
    }

    public enum GameplayEffectDurationCalculationPolicy
    {
        MagnitudeIsMultiplicative,
        MagnitudeIsAdditive,
        MagnitudeIsDuration
    }

    public enum GameplayEffectPeriodCalculationPolicy
    {
        MagnitudeIsTicks,
        MagnitudeIsPeriod,
        MagnitudeIsPeriodRatio
    }
}
