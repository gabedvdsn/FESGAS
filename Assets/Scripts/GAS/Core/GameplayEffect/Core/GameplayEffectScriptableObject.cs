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

        public override GameplayEffectSpec Generate(IEffectDerivation derivation, GASComponent target)
        {
            GameplayEffectSpec spec = new GameplayEffectSpec(this, derivation, target);
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
                DurationSpecification.Ticks = Mathf.FloorToInt(DurationSpecification.Duration * (1 / GASRateNormals.DEFAULT_TICK_PERIOD));
            }
        }

    }

    public enum GameplayEffectApplicationPolicy
    {
        Append,  // Create another instance of the effect independent of the existing one(s)
        Refresh,  // Refresh the duration of the effect
        Extend,  // Extend the duration of the effect
        Stack,  // Inject a duration-independent stack of the effect into the existing one 
        StackRefresh,  // Stack and refresh the duration of each stack
        StackExtend  // Stacks and extend the duration of each stack
    }

    public class GameplayEffectSpec : IAttributeDerivation
    {
        public GameplayEffectScriptableObject Base;
        public float Level;
        public float RelativeLevel;

        public IEffectDerivation Derivation;
        public GASComponent Source;
        public GASComponent Target;

        public Dictionary<AbstractMagnitudeModifierScriptableObject, AttributeValue?> SourceCapturedAttributes;

        public GameplayEffectSpec(GameplayEffectScriptableObject GameplayEffect, IEffectDerivation derivation, GASComponent target)
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

        public SourcedModifiedAttributeValue SourcedImpact(IAttributeDerivation baseDerivation, AttributeValue attributeValue)
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
        public GASComponent GetSource()
        {
            return Source;
        }
        public EImpactType GetImpactType()
        {
            return Base.ImpactSpecification.ImpactType;
        }
        public bool RetainAttributeImpact()
        {
            return false;
        }
        public void TrackImpact(AttributeValue impactValue)
        {
            // Specs do not track their own impact
        }
        public bool TryGetTrackedImpact(out AttributeValue impactValue)
        {
            impactValue = default;
            return false;
        }
    }

    public interface IEffectDerivation
    {
        public GASComponent GetOwner();
        public GameplayTagScriptableObject GetContextTag();
        public int GetLevel();
        public void SetLevel(int level);
        public float GetRelativeLevel();
        public string GetName();

        public static SourceEffectDerivation GenerateSourceDerivation(GASComponent source)
        {
            return new SourceEffectDerivation(source);
        }
    }

    public class SourceEffectDerivation : IEffectDerivation
    {
        private GASComponent Owner;

        public SourceEffectDerivation(GASComponent owner)
        {
            Owner = owner;
        }

        public GASComponent GetOwner()
        {
            return Owner;
        }
        public GameplayTagScriptableObject GetContextTag()
        {
            return Owner.Data.NameTag;
        }
        public int GetLevel()
        {
            return Owner.Data.Level;
        }
        public void SetLevel(int level)
        {
            Owner.Data.Level = level;
        }
        public float GetRelativeLevel()
        {
            return (Owner.Data.Level - 1) / (float)(Owner.Data.MaxLevel - 1);
        }
        public string GetName()
        {
            return Owner.Data.DistinctName;
        }
    }

}
