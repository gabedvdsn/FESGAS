namespace FESGameplayAbilitySystem
{
    public abstract class AbstractGameplayEffectShelfContainer : IAttributeDerivation
    {
        public GameplayEffectSpec Spec;
        public bool Ongoing;
        public bool Valid;

        protected float totalDuration;
        protected float periodDuration;

        public float TotalDuration => totalDuration;
        public abstract float DurationRemaining { get; }
        public float PeriodDuration => periodDuration;
        
        public abstract float TimeUntilPeriodTick { get; }
        
        private SourcedModifiedAttributeValue TrackedImpact;

        protected AbstractGameplayEffectShelfContainer(GameplayEffectSpec spec, bool ongoing)
        {
            Spec = spec;
            Ongoing = ongoing;
            
            TrackedImpact = new SourcedModifiedAttributeValue(Spec, 0, 0);
        }
        
        public void TrackImpact(SourcedModifiedAttributeValue smav)
        {
            TrackedImpact = TrackedImpact.Combine(smav);
        }

        public void SetTotalDuration(float duration)
        {
            totalDuration = duration;
            if (DurationRemaining > totalDuration) SetDurationRemaining(totalDuration);
        }
        public abstract void SetDurationRemaining(float duration);

        public void SetPeriodDuration(float duration)
        {
            periodDuration = duration;
            if (TimeUntilPeriodTick > periodDuration) SetTimeUntilPeriodTick(periodDuration);
        }
        public abstract void SetTimeUntilPeriodTick(float duration);

        public abstract void UpdateTimeRemaining(float deltaTime);
        public abstract void TickPeriodic(float deltaTime, out int executeTicks);
        
        public abstract void Refresh();
        public abstract void Extend(float duration);
        public abstract void Stack();

        public virtual void OnRemove()
        {
            Valid = false;
            if (Spec.Base.ImpactSpecification.ReverseImpactOnRemoval)
            {
                Spec.Target.AttributeSystem.ModifyAttribute(Spec.Base.ImpactSpecification.AttributeTarget, TrackedImpact.Negate());
            }
        }

        public IEffectDerivation GetEffectDerivation()
        {
            return Spec.GetEffectDerivation();
        }
        public GASComponent GetSource()
        {
            return Spec.GetSource();
        }
        public EImpactType GetImpactType()
        {
            return Spec.GetImpactType();
        }
        public bool RetainAttributeImpact()
        {
            return false;
        }
        
    }
}
