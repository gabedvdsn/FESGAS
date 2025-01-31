using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace FESGameplayAbilitySystem
{
    [Serializable]
    public class GameplayEffectDurationSpecification
    {
        public GameplayEffectDurationPolicy DurationPolicy;
        public bool TickOnApplication;

        [Space] 
        
        public float Duration;
        public AbstractMagnitudeModifierScriptableObject DurationCalculation;
        [FormerlySerializedAs("DurationCalculationPolicy")] public MagnitudeOperation DurationCalculationOperation;

        [Space] 
        
        public int Ticks;
        public AbstractMagnitudeModifierScriptableObject TickCalculation;
        public MagnitudeOperation TickCalculationOperation;
        public TickCalculationRounding Rounding;
        public bool UseDefaultTickRate;

        public void ApplyDurationSpecifications(GameplayEffectShelfContainer container)
        {
            // Apply duration
            if (DurationPolicy != GameplayEffectDurationPolicy.Instant)
            {
                container.TotalDuration = GetTotalDuration(container.Spec);

                container.DurationRemaining = container.TotalDuration;
            }
            
            // Apply period
            int numTicks = GetTicks(container.Spec);

            if (numTicks > 0)
            {
                container.PeriodDuration = container.TotalDuration / numTicks;
                container.TimeUntilPeriodTick = container.PeriodDuration;
            }
            else
            {
                container.PeriodDuration = float.MaxValue;
                container.TimeUntilPeriodTick = float.MaxValue;
            }
            
            //container.TimeUntilPeriodTick = TickOnApplication ? 0f : container.PeriodDuration;
        }

        public float GetTotalDuration(GameplayEffectSpec spec)
        {
            DurationCalculation.Initialize(spec);
            return DurationCalculationOperation switch
            {
                MagnitudeOperation.Multiply => Duration * DurationCalculation.Evaluate(spec),
                MagnitudeOperation.Add => Duration + DurationCalculation.Evaluate(spec),
                MagnitudeOperation.UseMagnitude => Duration,
                MagnitudeOperation.UseCalculation => DurationCalculation.Evaluate(spec),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public int GetTicks(GameplayEffectSpec spec)
        {
            TickCalculation.Initialize(spec);
            
            float floatTicks = TickCalculationOperation switch
            {
                MagnitudeOperation.Multiply => Ticks * TickCalculation.Evaluate(spec),
                MagnitudeOperation.Add => Ticks + TickCalculation.Evaluate(spec),
                MagnitudeOperation.UseMagnitude => Ticks,
                MagnitudeOperation.UseCalculation => TickCalculation.Evaluate(spec),
                _ => throw new ArgumentOutOfRangeException()
            };
            int numTicks = Rounding switch
            {

                TickCalculationRounding.Floor => Mathf.FloorToInt(floatTicks),
                TickCalculationRounding.Ceil => Mathf.CeilToInt(floatTicks),
                _ => throw new ArgumentOutOfRangeException()
            };

            return numTicks;
        }
        
    }
    
    
    
    public enum GameplayEffectDurationPolicy
    {
        Instant,
        Infinite,
        Durational
    }

    public enum TickCalculationRounding
    {
        Floor,
        Ceil
    }
}
