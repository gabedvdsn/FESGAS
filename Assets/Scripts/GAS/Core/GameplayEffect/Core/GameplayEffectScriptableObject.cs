using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.Serialization;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(menuName = "FESGAS/Gameplay Effect", fileName = "New Gameplay Effect")]
    public class GameplayEffectScriptableObject : AbstractGameplayEffectScriptableObject
    {
        [Header("Gameplay Effect")]
        
        public GameplayTagScriptableObject Identifier;
        public GameplayTagScriptableObject[] GrantedTags;

        [Header("Specifications")] 
        
        public GameplayEffectImpactSpecification ImpactSpecification;
        public GameplayEffectDurationSpecification DurationSpecification;
        
        [Header("Requirements")]
        
        public GameplayEffectRequirements SourceRequirements;
        public GameplayEffectRequirements TargetRequirements;

        [Header("Interactions")] 
        
        public GameplayTagScriptableObject[] RemoveEffectsWithTag;

        public override GameplayEffectSpec Generate(AbilitySpec ability, GASComponent target)
        {
            GameplayEffectSpec spec = new GameplayEffectSpec(this, ability, target);
            ApplyImpactSpecification(spec);

            return spec;
        }
        public override void ApplyImpactSpecification(GameplayEffectSpec spec)
        {
            ImpactSpecification.ApplyImpactSpecifications(spec);
        }

        private void OnValidate()
        {
            if (DurationSpecification.UseDefaultTickRate && DurationSpecification.Duration > 0f)
            {
                DurationSpecification.Ticks = Mathf.FloorToInt(DurationSpecification.Duration * (1 / StaticRateNormals.DEFAULT_TICK_PERIOD));
            }
        }

    }

    public class GameplayEffectSpec
    {
        public GameplayEffectScriptableObject Base;
        public float Level;
        public float RelativeLevel;

        public AbilitySpec Ability;
        public GASComponent Source;
        public GASComponent Target;

        public Dictionary<AbstractMagnitudeModifierScriptableObject, AttributeValue?> SourceCapturedAttributes;

        public GameplayEffectSpec(GameplayEffectScriptableObject GameplayEffect, AbilitySpec ability, GASComponent target)
        {
            Base = GameplayEffect;
            Ability = ability;
            
            Source = Ability.Owner;
            Target = target;

            Level = Ability.Level;
            RelativeLevel = Ability.RelativeLevel;

            SourceCapturedAttributes = new Dictionary<AbstractMagnitudeModifierScriptableObject, AttributeValue?>();
        }

        public SourcedModifiedAttributeValue ToSourcedModified(AttributeValue attributeValue)
        {
            float magnitude = Base.ImpactSpecification.GetMagnitude(this);
            float currValue = attributeValue.CurrentValue;
            float baseValue = attributeValue.BaseValue;
            
            switch (Base.ImpactSpecification.ImpactOperation)
            {
                case CalculationOperation.Add:
                    switch (Base.ImpactSpecification.ValueTarget)
                    {
                        case EffectImpactTarget.Current:
                            currValue += magnitude;
                            break;
                        case EffectImpactTarget.Base:
                            baseValue += magnitude;
                            break;
                        case EffectImpactTarget.CurrentAndBase:
                            currValue += magnitude;
                            baseValue += magnitude;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
                case CalculationOperation.Multiply:
                    switch (Base.ImpactSpecification.ValueTarget)
                    {
                        case EffectImpactTarget.Current:
                            currValue *= magnitude;
                            break;
                        case EffectImpactTarget.Base:
                            baseValue *= magnitude;
                            break;
                        case EffectImpactTarget.CurrentAndBase:
                            currValue *= magnitude;
                            baseValue *= magnitude;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
                case CalculationOperation.Override:
                    switch (Base.ImpactSpecification.ValueTarget)
                    {
                        case EffectImpactTarget.Current:
                            currValue = magnitude;
                            break;
                        case EffectImpactTarget.Base:
                            baseValue = magnitude;
                            break;
                        case EffectImpactTarget.CurrentAndBase:
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

            return new SourcedModifiedAttributeValue(
                Ability,
                currValue - attributeValue.CurrentValue,
                baseValue - attributeValue.BaseValue
            );
        }
    }

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

        public GameplayEffectShelfContainer(GameplayEffectSpec spec, bool ongoing)
        {
            Spec = spec;
            Ongoing = ongoing;
            Valid = true;
            
            Spec.Base.DurationSpecification.ApplyDurationSpecifications(this);
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
            Debug.Log(modifiedAttributeValue);
            TrackedImpact = TrackedImpact.Combine(modifiedAttributeValue);
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
