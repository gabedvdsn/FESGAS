using System;
using System.Collections.Generic;

namespace FESGameplayAbilitySystem
{
    public class GameplayEffectSpec : IAttributeImpactDerivation
    {
        public IEffectBase Base;
        public float Level;
        public float RelativeLevel;

        public IEffectDerivation Derivation;
        public ISource Source;
        public GASComponentBase Target;

        private List<AbstractEffectWorkerScriptableObject> Workers;

        public Dictionary<AbstractMagnitudeModifierScriptableObject, AttributeValue?> SourceCapturedAttributes;

        public GameplayEffectSpec(IEffectBase GameplayEffect, IEffectDerivation derivation, GASComponentBase target)
        {
            Base = GameplayEffect;
            Derivation = derivation;
            
            Source = Derivation.GetOwner();
            Target = target;

            Level = Derivation.GetLevel();
            RelativeLevel = Derivation.GetRelativeLevel();

            Workers = Base.GetEffectWorkers();

            SourceCapturedAttributes = new Dictionary<AbstractMagnitudeModifierScriptableObject, AttributeValue?>();
        }
        
        public SourcedModifiedAttributeValue SourcedImpact(AttributeValue attributeValue)
        {
            AttributeValue impactValue = AttributeImpact(attributeValue);
            return new SourcedModifiedAttributeValue(
                this,
                impactValue.CurrentValue,
                impactValue.BaseValue
            );
        }

        public SourcedModifiedAttributeValue SourcedImpact(IAttributeImpactDerivation baseDerivation, AttributeValue attributeValue)
        {
            AttributeValue impactValue = AttributeImpact(attributeValue);
            return new SourcedModifiedAttributeValue(
                this,
                baseDerivation,
                impactValue.CurrentValue,
                impactValue.BaseValue
            );
        }
        
        private AttributeValue AttributeImpact(AttributeValue attributeValue)
        {
            float magnitude = Base.GetMagnitude(this);
            float currValue = attributeValue.CurrentValue;
            float baseValue = attributeValue.BaseValue;
            
            switch (Base.GetImpactOperation())
            {
                case ECalculationOperation.Add:
                    switch (Base.GetTargetImpact())
                    {
                        case EEffectImpactTarget.Current:
                            currValue += magnitude;
                            break;
                        case EEffectImpactTarget.Base:
                            baseValue += magnitude;
                            break;
                        case EEffectImpactTarget.CurrentAndBase:
                            currValue += magnitude;
                            baseValue += magnitude;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
                case ECalculationOperation.Multiply:
                    switch (Base.GetTargetImpact())
                    {
                        case EEffectImpactTarget.Current:
                            currValue *= magnitude;
                            break;
                        case EEffectImpactTarget.Base:
                            baseValue *= magnitude;
                            break;
                        case EEffectImpactTarget.CurrentAndBase:
                            currValue *= magnitude;
                            baseValue *= magnitude;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
                case ECalculationOperation.Override:
                    switch (Base.GetTargetImpact())
                    {
                        case EEffectImpactTarget.Current:
                            currValue = magnitude;
                            break;
                        case EEffectImpactTarget.Base:
                            baseValue = magnitude;
                            break;
                        case EEffectImpactTarget.CurrentAndBase:
                            currValue = magnitude;
                            baseValue = magnitude;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return new AttributeValue(
                currValue - attributeValue.CurrentValue,
                baseValue - attributeValue.BaseValue
            );
        }

        public AttributeScriptableObject GetAttribute()
        {
            return Base.GetAttributeTarget();
        }
        public IEffectDerivation GetEffectDerivation()
        {
            return Derivation;
        }
        public ISource GetSource()
        {
            return Source;
        }
        public ITarget GetTarget()
        {
            return Target;
        }
        public EImpactType GetImpactType()
        {
            return Base.GetImpactType();
        }
        public bool RetainAttributeImpact()
        {
            return false;
        }
        public void TrackImpact(AbilityImpactData impactData)
        {
            // Specs do not track their own impact (tracked in effect containers)
        }
        public bool TryGetTrackedImpact(out AttributeValue impactValue)
        {
            impactValue = default;
            return false;
        }
        public bool TryGetLastTrackedImpact(out AttributeValue impactValue)
        {
            impactValue = default;
            return false;
        }
        public List<GameplayTagScriptableObject> GetContextTags()
        {
            return Derivation.GetContextTags();
        }
        public void RunEffectApplicationWorkers()
        {
            foreach (AbstractEffectWorkerScriptableObject worker in Workers) worker.OnEffectApplication(this);
        }
        public void RunEffectTickWorkers()
        {
            // Specs never run this method (because non-durational specs, i.e. without containers, are never ticked)
            // foreach (AbstractEffectWorkerScriptableObject worker in Workers) worker.OnEffectTick(this);
        }
        public void RunEffectRemovalWorkers()
        {
            foreach (AbstractEffectWorkerScriptableObject worker in Workers) worker.OnEffectRemoval(this);
        }
        public void RunEffectImpactWorkers(AbilityImpactData impactData)
        {
            foreach (AbstractEffectWorkerScriptableObject worker in Workers) worker.OnEffectImpact(impactData);
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
