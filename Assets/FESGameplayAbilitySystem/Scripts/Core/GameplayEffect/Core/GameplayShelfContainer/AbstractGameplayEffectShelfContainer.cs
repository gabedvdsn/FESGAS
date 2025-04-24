using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public abstract class AbstractGameplayEffectShelfContainer : IAttributeImpactDerivation
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
        private AttributeValue LastTrackedImpact;

        private List<AbstractEffectWorkerScriptableObject> Workers;

        protected AbstractGameplayEffectShelfContainer(GameplayEffectSpec spec, bool ongoing)
        {
            Spec = spec;
            Ongoing = ongoing;

            Workers = spec.Base.GetEffectWorkers();
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
            if (Spec.Base.GetReverseImpactOnRemoval())
            {
                AttributeValue negatedImpact = TrackedImpact.Negate();
                Spec.Target.AttributeSystem.ModifyAttribute(
                    Spec.Base.GetAttributeTarget(), 
                    new SourcedModifiedAttributeValue(Spec, this, negatedImpact.CurrentValue, negatedImpact.BaseValue, false));
            }
            
            foreach (var containedEffect in Spec.Base.GetContainedEffects(EApplyDuringRemove.OnRemove))
            {
                Spec.GetSource().ApplyGameplayEffect(Spec.Derivation, containedEffect);
            }
        }

        public AttributeScriptableObject GetAttribute()
        {
            return Spec.Base.GetAttributeTarget();
        }
        public IEffectDerivation GetEffectDerivation()
        {
            return Spec.GetEffectDerivation();
        }
        public GASComponentBase GetSource()
        {
            return Spec.GetSource();
        }
        public GASComponentBase GetTarget()
        {
            return Spec.Target;
        }
        public EImpactType GetImpactType()
        {
            return Spec.GetImpactType();
        }
        public bool RetainAttributeImpact()
        {
            return false;
        }
        public void TrackImpact(AbilityImpactData impactData)
        {
            TrackedImpact = TrackedImpact.Combine(impactData.RealImpact);
            LastTrackedImpact = impactData.RealImpact;
        }
        public bool TryGetTrackedImpact(out AttributeValue impactValue)
        {
            impactValue = TrackedImpact;
            return true;
        }
        public bool TryGetLastTrackedImpact(out AttributeValue impactValue)
        {
            impactValue = LastTrackedImpact;
            return true;
        }
        public List<GameplayTagScriptableObject> GetContextTags()
        {
            return Spec.GetContextTags();
        }
        public void RunEffectApplicationWorkers()
        {
            foreach (var worker in Workers) worker.OnEffectApplication(this);
        }
        public void RunEffectTickWorkers()
        {
            foreach (var worker in Workers) worker.OnEffectTick(this);
        }
        public void RunEffectRemovalWorkers()
        {
            foreach (var worker in Workers) worker.OnEffectRemoval(this);
        }
        public void RunEffectImpactWorkers(AbilityImpactData impactData)
        {
            foreach (var worker in Workers) worker.OnEffectImpact(impactData);
        }

        public override string ToString()
        {
            return Spec.Base.ToString();
        }

    }
}
