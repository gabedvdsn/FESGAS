namespace FESGameplayAbilitySystem
{
    public struct GameplayEffectDuration
    {
        public float TotalDuration;
        public float DurationRemaining;

        public GameplayEffectDuration(float totalDuration, float durationRemaining)
        {
            TotalDuration = totalDuration;
            DurationRemaining = durationRemaining;
        }

        public override string ToString()
        {
            return $"{DurationRemaining}/{TotalDuration}";
        }
    }
}
