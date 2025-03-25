using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public abstract class AbstractGameplayEffectShelfContainer : IAttributeDerivation, ITaggable
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
        
        private AttributeValue TrackedImpact;

        protected AbstractGameplayEffectShelfContainer(GameplayEffectSpec spec, bool ongoing)
        {
            Spec = spec;
            Ongoing = ongoing;
    
            TrackedImpact = default;

            Valid = true;
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

        public virtual int GetStacks() => 1;

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
                AttributeValue negatedImpact = TrackedImpact.Negate();
                Spec.Target.AttributeSystem.ModifyAttribute(
                    Spec.Base.ImpactSpecification.AttributeTarget, 
                    new SourcedModifiedAttributeValue(Spec, negatedImpact.CurrentValue, negatedImpact.BaseValue));
            }
        }

        public AttributeScriptableObject GetAttribute()
        {
            return Spec.Base.ImpactSpecification.AttributeTarget;
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
        public void TrackImpact(AttributeValue impactValue)
        {
            TrackedImpact = TrackedImpact.Combine(impactValue);
        }
        public bool TryGetTrackedImpact(out AttributeValue impactValue)
        {
            impactValue = TrackedImpact;
            return true;
        }

        public override string ToString()
        {
            return Spec.Base.ToString();
        }
        public IEnumerable<GameplayTagScriptableObject> GetTags()
        {
            return Spec.GetTags();
        }
        public bool PersistentTags()
        {
            return true;
        }

    }
}
