using System.Linq;

namespace FESGameplayAbilitySystem.Core
{
    public static class StaticSignPolicy
    {
        public static SignPolicy DeterminePolicy(params float[] magnitudes)
        {
            float sum = magnitudes.Sum();
            return sum switch
            {
                > 0 => SignPolicy.Positive,
                < 0 => SignPolicy.Negative,
                0 when magnitudes.Any(mag => mag != 0) => SignPolicy.ZeroBiased,
                _ => SignPolicy.ZeroNeutral
            };
        }
    }
}
