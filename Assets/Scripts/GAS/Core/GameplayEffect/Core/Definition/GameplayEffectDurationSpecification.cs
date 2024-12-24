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
        [FormerlySerializedAs("TickCalculationPolicy")] public MagnitudeOperation TickCalculationOperation;
        public TickCalculationRounding Rounding;
        [FormerlySerializedAs("UseDefaultRate")] public bool UseDefaultTickRate;

        public void ApplyDurationSpecifications(GameplayEffectShelfContainer container)
        {
            // Apply duration
            if (DurationPolicy != GameplayEffectDurationPolicy.Instant)
            {
                DurationCalculation.Initialize(container.Spec);

                container.TotalDuration = DurationCalculationOperation switch
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
            float floatTicks = TickCalculationOperation switch
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
