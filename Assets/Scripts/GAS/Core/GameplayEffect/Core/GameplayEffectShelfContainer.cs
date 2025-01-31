namespace FESGameplayAbilitySystem
{
    public class GameplayEffectShelfContainer
    {
        public GameplayEffectSpec Spec;
        public bool Ongoing;
        public bool Valid;
        
        public float TotalDuration;
        public float DurationRemaining;

        public float PeriodDuration;
        public float TimeUntilPeriodTick;

        public SourcedModifiedAttributeValue TrackedImpact;

        private GameplayEffectShelfContainer(GameplayEffectSpec spec, bool ongoing)
        {
            Spec = spec;
            Ongoing = ongoing;
            Valid = true;

            TrackedImpact = new SourcedModifiedAttributeValue(Spec, 0, 0);
            Spec.Base.DurationSpecification.ApplyDurationSpecifications(this);
        }

        public static GameplayEffectShelfContainer Generate(GameplayEffectSpec spec, bool ongoing)
        {
            return new GameplayEffectShelfContainer(spec, ongoing);
        }

        public void UpdateTimeRemaining(float deltaTime)
        {
            DurationRemaining -= deltaTime;
        }

        public void TickPeriodic(float deltaTime, out bool executeTick)
        {
            TimeUntilPeriodTick -= deltaTime;
            if (TimeUntilPeriodTick <= 0f)
            {
                TimeUntilPeriodTick += PeriodDuration;
                executeTick = true;
            }
            else
            {
                executeTick = false;
            }
        }

        public void TrackImpact(SourcedModifiedAttributeValue modifiedAttributeValue)
        {
            TrackedImpact = TrackedImpact.Combine(modifiedAttributeValue);
        }

        public void Refresh()
        {
            DurationRemaining = TotalDuration;
            TimeUntilPeriodTick = PeriodDuration;
        }

        public void Extend(float duration)
        {
            TotalDuration += duration;
            DurationRemaining += duration;
        }

        public void OnRemove()
        {
            Valid = false;
            if (Spec.Base.ImpactSpecification.ReverseImpactOnRemoval)
            {
                Spec.Target.AttributeSystem.ModifyAttribute(Spec.Base.ImpactSpecification.AttributeTarget, TrackedImpact.Negate());
            }
        }
    }
}
