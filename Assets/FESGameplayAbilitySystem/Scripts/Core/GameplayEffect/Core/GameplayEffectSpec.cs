﻿using System;
using System.Collections.Generic;

namespace FESGameplayAbilitySystem
{
    public class GameplayEffectSpec : IAttributeImpactDerivation, ITaggable
    {
        public GameplayEffectScriptableObject Base;
        public float Level;
        public float RelativeLevel;

        public IEffectDerivation Derivation;
        public GASComponentBase Source;
        public GASComponentBase Target;

        public Dictionary<AbstractMagnitudeModifierScriptableObject, AttributeValue?> SourceCapturedAttributes;

        public GameplayEffectSpec(GameplayEffectScriptableObject GameplayEffect, IEffectDerivation derivation, GASComponentBase target)
        {
            Base = GameplayEffect;
            Derivation = derivation;
            
            Source = Derivation.GetOwner();
            Target = target;

            Level = Derivation.GetLevel();
            RelativeLevel = Derivation.GetRelativeLevel();

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
            float magnitude = Base.ImpactSpecification.GetMagnitude(this);
            float currValue = attributeValue.CurrentValue;
            float baseValue = attributeValue.BaseValue;
            
            switch (Base.ImpactSpecification.ImpactOperation)
            {
                case ECalculationOperation.Add:
                    switch (Base.ImpactSpecification.ValueTarget)
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
                    switch (Base.ImpactSpecification.ValueTarget)
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
                    switch (Base.ImpactSpecification.ValueTarget)
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
            return Base.ImpactSpecification.AttributeTarget;
        }
        public IEffectDerivation GetEffectDerivation()
        {
            return Derivation;
        }
        public GASComponentBase GetSource()
        {
            return Source;
        }
        public GASComponentBase GetTarget()
        {
            return Target;
        }
        public EImpactType GetImpactType()
        {
            return Base.ImpactSpecification.ImpactType;
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
            foreach (AbstractEffectWorkerScriptableObject worker in Base.Workers) worker.OnEffectApplication(this);
        }
        public void RunEffectRemovalWorkers()
        {
            foreach (AbstractEffectWorkerScriptableObject worker in Base.Workers) worker.OnEffectRemoval(this);
        }
        public void RunEffectWorkers(AbilityImpactData impactData)
        {
            foreach (AbstractEffectWorkerScriptableObject worker in Base.Workers) worker.OnEffectImpact(impactData);
        }
        public IEnumerable<GameplayTagScriptableObject> GetTags()
        {
            return Base.GrantedTags;
        }
        public bool PersistentTags()
        {
            return false;
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
