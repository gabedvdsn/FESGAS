using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.Serialization;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(menuName = "FESGAS/Gameplay Effect", fileName = "New Gameplay Effect")]
    public class GameplayEffectScriptableObject : ScriptableObject
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
        
        public GameplayEffectSpec Generate(GASComponent Source, GASComponent Target, float Level)
        {
            GameplayEffectSpec spec = new GameplayEffectSpec(this, Source, Target, Level);
            ImpactSpecification.ApplyImpactSpecifications(spec);

            return spec;
        }

        public GameplayEffectSpec Generate(AbilitySpec ability, GASComponent target)
        {
            GameplayEffectSpec spec = new GameplayEffectSpec(this, ability, target);
            ImpactSpecification.ApplyImpactSpecifications(spec);

            return spec;
        }

    }

    public class GameplayEffectSpec
    {
        public GameplayEffectScriptableObject Base;
        public float Level;
        public float RelativeLevel;
        
        public GASComponent Source;
        public GASComponent Target;

        public Dictionary<AbstractGameplayEffectCalculationScriptableObject, AttributeValue?> SourceCapturedAttributes;

        public GameplayEffectSpec(GameplayEffectScriptableObject gameplayEffect, GASComponent source, GASComponent target, float level)
        {
            Base = gameplayEffect;
            Source = source;
            Target = target;
            
            Level = level;
            RelativeLevel = level - 1;

            SourceCapturedAttributes = new Dictionary<AbstractGameplayEffectCalculationScriptableObject, AttributeValue?>();
        }

        public GameplayEffectSpec(GameplayEffectScriptableObject GameplayEffect, AbilitySpec ability, GASComponent target)
        {
            Base = GameplayEffect;
            Source = ability.Owner;
            Target = target;

            Level = ability.Level;
            RelativeLevel = ability.RelativeLevel;

            SourceCapturedAttributes = new Dictionary<AbstractGameplayEffectCalculationScriptableObject, AttributeValue?>();
        }

        public ModifiedAttributeValue ToModifiedAttributeValue(AttributeValue attributeValue)
        {
            float magnitude = Base.ImpactSpecification.GetMagnitude(this);
            float currValue = attributeValue.CurrentValue;
            float baseValue = attributeValue.BaseValue;
            
            switch (Base.ImpactSpecification.ImpactOperation)
            {
                case CalculationOperation.Add:
                    switch (Base.ImpactSpecification.ValueTarget)
                    {
                        case EffectImpactTargetCalculation.Current:
                            currValue += magnitude;
                            break;
                        case EffectImpactTargetCalculation.Base:
                            baseValue += magnitude;
                            break;
                        case EffectImpactTargetCalculation.CurrentAndBase:
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
                        case EffectImpactTargetCalculation.Current:
                            currValue *= magnitude;
                            break;
                        case EffectImpactTargetCalculation.Base:
                            baseValue *= magnitude;
                            break;
                        case EffectImpactTargetCalculation.CurrentAndBase:
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
                        case EffectImpactTargetCalculation.Current:
                            currValue = magnitude;
                            break;
                        case EffectImpactTargetCalculation.Base:
                            baseValue = magnitude;
                            break;
                        case EffectImpactTargetCalculation.CurrentAndBase:
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

            return new ModifiedAttributeValue(
                this,
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
    }
}
