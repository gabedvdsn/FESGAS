using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public static class GASRateNormals
    {
        private const float DEF_SLOW_TICK_PERIOD = 2.5f;
        private const float DEF_MEDIUM_TICK_PERIOD = 5f;
        private const float DEF_FAST_TICK_PERIOD = 7.5f;
        private const float DEF_VERY_FAST_TICK_PERIOD = 10f;

        public static float GetDefaultTickRate(EDefaultTickRate rate)
        {
            return rate switch
            {
                EDefaultTickRate.None => 1f,
                EDefaultTickRate.Slow => DEF_SLOW_TICK_PERIOD,
                EDefaultTickRate.Normal => DEF_MEDIUM_TICK_PERIOD,
                EDefaultTickRate.Fast => DEF_FAST_TICK_PERIOD,
                EDefaultTickRate.VeryFast => DEF_VERY_FAST_TICK_PERIOD,
                _ => throw new ArgumentOutOfRangeException(nameof(rate), rate, null)
            };
        }
    }
}
