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
                DurationSpecification.Ticks = Mathf.FloorToInt(DurationSpecification.Duration * (1 / StaticRateNormals.DEFAULT_TICK_PERIOD));
            }
        }

    }

    public enum GameplayEffectApplicationPolicy
    {
        Refresh,  // Refresh the duration of the effect
        Extend,  // Extend the duration of the effect
        Append,  // Append another instance of the effect independent of existing one(s)
        StackRefresh,  // Stacks and refreshes the duration of the effect
        StackExtend  // Stacks and extends the duration of the effect
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
                this,
                currValue - attributeValue.CurrentValue,
                baseValue - attributeValue.BaseValue
            );
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
