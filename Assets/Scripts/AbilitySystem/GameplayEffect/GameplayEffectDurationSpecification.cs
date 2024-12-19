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
        public AbstractGameplayEffectCalculationScriptableObject DurationCalculation;
        public MagnitudeOperation DurationCalculationPolicy;

        [Space] 
        
        public int Ticks;
        public AbstractGameplayEffectCalculationScriptableObject TickCalculation;
        public MagnitudeOperation TickCalculationPolicy;
        public TickCalculationRounding Rounding;

        public void ApplyDurationSpecifications(GameplayEffectShelfContainer container)
        {
            // Apply duration
            if (DurationPolicy == GameplayEffectDurationPolicy.Durational)
            {
                DurationCalculation.Initialize(container.Spec);

                container.TotalDuration = DurationCalculationPolicy switch
                {
                    MagnitudeOperation.Multiply => Duration * DurationCalculation.Evaluate(container.Spec),
                    MagnitudeOperation.Add => Duration + DurationCalculation.Evaluate(container.Spec),
                    MagnitudeOperation.UseMagnitude => Duration,
                    MagnitudeOperation.UseCalculation => DurationCalculation.Evaluate(container.Spec),
                    _ => throw new ArgumentOutOfRangeException()
                };

                container.DurationRemaining = container.TotalDuration;
            }
            
            TickCalculation.Initialize(container.Spec);
            
            // Apply period
            float floatTicks = TickCalculationPolicy switch
            {
                MagnitudeOperation.Multiply => Ticks * TickCalculation.Evaluate(container.Spec),
                MagnitudeOperation.Add => Ticks + TickCalculation.Evaluate(container.Spec),
                MagnitudeOperation.UseMagnitude => Ticks,
                MagnitudeOperation.UseCalculation => TickCalculation.Evaluate(container.Spec),
                _ => throw new ArgumentOutOfRangeException()
            };
            int numTicks = Rounding switch
            {

                TickCalculationRounding.Floor => Mathf.FloorToInt(floatTicks),
                TickCalculationRounding.Ceil => Mathf.CeilToInt(floatTicks),
                _ => throw new ArgumentOutOfRangeException()
            };

            container.PeriodDuration = container.TotalDuration / numTicks;
            container.TimeUntilPeriodTick = container.PeriodDuration;
            
            //container.TimeUntilPeriodTick = TickOnApplication ? 0f : container.PeriodDuration;
        }
        
        public float TotalDuration(GameplayEffectSpec spec)
        {
            return DurationCalculationPolicy switch
            {
                MagnitudeOperation.Multiply => Duration * DurationCalculation.Evaluate(spec),
                MagnitudeOperation.Add => Duration + DurationCalculation.Evaluate(spec),
                MagnitudeOperation.UseMagnitude => Duration,
                MagnitudeOperation.UseCalculation => DurationCalculation.Evaluate(spec),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public int TotalTicks(GameplayEffectSpec spec)
        {
            float floatTicks = TickCalculationPolicy switch
            {
                MagnitudeOperation.Multiply => Ticks * TickCalculation.Evaluate(spec),
                MagnitudeOperation.Add => Ticks + TickCalculation.Evaluate(spec),
                MagnitudeOperation.UseMagnitude => Ticks,
                MagnitudeOperation.UseCalculation => TickCalculation.Evaluate(spec),
                _ => throw new ArgumentOutOfRangeException()
            };
            return Rounding switch
            {

                TickCalculationRounding.Floor => Mathf.FloorToInt(floatTicks),
                TickCalculationRounding.Ceil => Mathf.CeilToInt(floatTicks),
                _ => throw new ArgumentOutOfRangeException()
            };
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
