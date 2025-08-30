using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public static class FESGAS
    {
        public static class Settings
        {
            public static class EffectSettings
            {
                #region Preset Tick Rates
                
                private static Dictionary<Tag, float> presetTickRates = new();

                public static void AddPresetTickRate(Tag preset, float rate) => presetTickRates.SafeAdd(preset, rate, true);

                public static float GetPresetTickRate(Tag preset) => presetTickRates.TryGetValue(preset, out float rate) ? rate : -1f;
                
                #endregion
            }
            
        }
        
        public static class Modifiers
        {
            #region Normal
            
            public static AbstractMagnitudeModifier CreateSimpleModifier(AnimationCurve arg)
            {
                return new SimpleMagnitudeModifier
                {
                    Scaling = arg
                };
            }
            
            public static AbstractMagnitudeModifier CreateAttributeBackedModifier(
                AnimationCurve arg, 
                EEffectImpactTargetLimited scalingPolicy,
                Attribute captureAttribute
                )
            {
                return new AttributeBackedMagnitudeModifier()
                {
                    Scaling = arg,
                    ScalingPolicy =  scalingPolicy,
                    CaptureAttribute = captureAttribute
                };
            }
            
            public static AbstractMagnitudeModifier CreateModifierGroup(
                MagnitudeModifierGroupMember[] arg, 
                EValueCollisionPolicy collisionPolicy
                )
            {
                return new MagnitudeModifierGroup
                {
                    Calculations = arg,
                    OverrideMemberCollisionPolicy = collisionPolicy
                };
            }
            
            #endregion
            
            #region Cached Attribute

            public static AbstractCachedMagnitudeModifier CreateCachedSimpleModifier(AnimationCurve arg)
            {
                return new AttrCacheSimpleMagnitudeModifier
                {
                    Scaling = arg
                };
            }

            public static AbstractCachedMagnitudeModifier CreateCachedAttributeBackedModifier(AnimationCurve arg, EEffectImpactTargetLimited scalingPolicy, Attribute attribute)
            {
                return new AttrCacheBackedMagnitudeModifier
                {
                    Scaling = arg,
                    ScalingPolicy = scalingPolicy,
                    CaptureAttribute = attribute
                };
            }

            public static AbstractCachedMagnitudeModifier CreateCachedModifierGroup(CachedMagnitudeModifierGroupMember[] arg, EValueCollisionPolicy collisionPolicy)
            {
                return new CachedMagnitudeModifierGroup
                {
                    Calculations = arg,
                    OverrideMemberCollisionPolicy = collisionPolicy
                };
            }
            
            #endregion
        }

        public static class Effects
        {
            public static GameplayEffect CreateGameplayEffect(
                GameplayEffectDefinition definition,
                GameplayEffectTags tags,
                GameplayEffectImpactSpecification impact,
                GameplayEffectDurationSpecification duration,
                AbstractEffectWorker[] workers,
                TagRequirements sourceReqs,
                TagRequirements targetReqs
                )
            {
                return new GameplayEffect
                {
                    Definition = definition,
                    Tags = tags,
                    ImpactSpecification = impact,
                    DurationSpecification = duration,
                    Workers = workers,
                    SourceRequirements = sourceReqs,
                    TargetRequirements = targetReqs
                };
            }

            public static GameplayEffectDefinition CreateEffectDefinition(
                string name,
                string description,
                Tag visibility,
                Sprite icon
            )
            {
                return new GameplayEffectDefinition()
                {
                    Name = name,
                    Description = description,
                    Visibility = visibility,
                    Icon = icon
                };
            }

            public static GameplayEffectTags CreateEffectTags(
                Tag assetTag,
                Tag[] context,
                Tag[] granted
            )
            {
                return new GameplayEffectTags()
                {
                    AssetTag = assetTag,
                    ContextTags = context,
                    GrantedTags = granted
                };
            }

            public static GameplayEffectImpactSpecification CreateEffectImpactSpec(
                Attribute attribute,
                EEffectImpactTarget target,
                ECalculationOperation operation,
                EAffiliationPolicy affiliationPolicy,
                EImpactType impactType,
                bool reverse,
                EEffectReApplicationPolicy reApplicationPolicy,
                float magnitude,
                AbstractMagnitudeModifier modifier,
                EMagnitudeOperation magnitudeOperation,
                ContainedEffectPacket[] contained
            )
            {
                return new GameplayEffectImpactSpecification()
                {
                    AttributeTarget = attribute,
                    TargetImpact = target,
                    ImpactOperation = operation,
                    AffiliationPolicy = affiliationPolicy,
                    ImpactType = impactType,
                    ReverseImpactOnRemoval = reverse,
                    ReApplicationPolicy = reApplicationPolicy,
                    Magnitude = magnitude,
                    MagnitudeCalculation = modifier,
                    MagnitudeCalculationOperation = magnitudeOperation,
                    Packets = contained
                };
            }

            public static GameplayEffectDurationSpecification CreateEffectDurationSpec(
                EEffectDurationPolicy durationPolicy,
                bool tickOnApplication,
                float duration,
                AbstractMagnitudeModifier modifier,
                EMagnitudeOperation modifierOperation,
                Tag deltaTimeSource,
                int ticks,
                AbstractMagnitudeModifier tickModifier,
                EMagnitudeOperation tickModifierOperation,
                ETickCalculationRounding tickRounding,
                Tag presetTickRatePolicy
            )
            {
                return new GameplayEffectDurationSpecification()
                {
                    DurationPolicy = durationPolicy,
                    TickOnApplication = tickOnApplication,
                    Duration = duration,
                    DurationCalculation = modifier,
                    DurationCalculationOperation = modifierOperation,
                    DeltaTimeSource = deltaTimeSource,
                    Ticks = ticks,
                    TickCalculation = tickModifier,
                    TickCalculationOperation = tickModifierOperation,
                    Rounding = tickRounding,
                    PresetTickRatePolicy = presetTickRatePolicy
                };
            }
        }
        
        public static class Tags
        {
            public static TagRequirements CreateEffectRequirement(
                AvoidRequireTagGroup application,
                AvoidRequireTagGroup ongoing,
                AvoidRequireTagGroup removal,
                TagRequirements[] nested
            )
            {
                return new TagRequirements
                {
                    ApplicationRequirements = application,
                    OngoingRequirements = ongoing,
                    RemovalRequirements = removal,
                    NestedRequirements = nested
                };
            }
        
            public static TagRequirements CreateEmptyRequirements()
            {
                return new TagRequirements();
            }

            public static AvoidRequireTagGroup CreateAvoidRequireTagGroup(
                Tag[] avoid,
                Tag[] require
            )
            {
                return new AvoidRequireTagGroup()
                {
                    AvoidTags = avoid,
                    RequireTags = require
                };
            }
        }
        
        public static class Abilities
        {
            public static Ability CreateAbility(
                AbilityDefinition definition,
                AbilityTags tags,
                AbilityProxySpecification proxy,
                int startingLevel,
                int maxLevel,
                bool ignoreWhenZero,
                GameplayEffect cost,
                GameplayEffect cooldown
            )
            {
                return new Ability()
                {
                    Definition = definition,
                    Tags = tags,
                    Proxy = proxy,
                    StartingLevel = startingLevel,
                    MaxLevel = maxLevel,
                    IgnoreWhenLevelZero = ignoreWhenZero,
                    Cost = cost,
                    Cooldown = cooldown
                };
            }

            public static AbilityDefinition CreateAbilityDefinition(
                string name,
                string description,
                EAbilityActivationPolicyExtended activation,
                bool activateImmediate,
                Sprite unlearned,
                Sprite normal,
                Sprite queued,
                Sprite cooldown
            )
            {
                return new AbilityDefinition()
                {
                    Name = name,
                    Description = description,
                    ActivationPolicy = activation,
                    ActivateImmediately = activateImmediate,
                    UnlearnedIcon = unlearned,
                    NormalIcon = normal,
                    QueuedIcon = queued,
                    OnCooldownIcon = cooldown
                };
            }

            public static AbilityTags CreateAbilityTags(
                Tag asset,
                Tag[] context,
                Tag[] passive,
                Tag[] active,
                AvoidRequireTagGroup source,
                AvoidRequireTagGroup target
            )
            {
                return new AbilityTags()
                {
                    AssetTag = asset,
                    ContextTags = context,
                    PassivelyGrantedTags = passive,
                    ActiveGrantedTags = active,
                    SourceRequirements = source,
                    TargetRequirements = target
                };
            }

            public static AbilityProxySpecification CreateAbilityProxySpec(
                AbstractTargetingProxyTask targeting,
                bool useImplicit,
                AbilityProxyStage[] stages
            )
            {
                return new AbilityProxySpecification()
                {
                    TargetingProxy = targeting,
                    UseImplicitTargeting = useImplicit,
                    Stages = stages
                };
            }
        }

        public static class ProxyTasks
        {
            //public AbstractAbilityProxyTask Create
        }

        public static class Workers
        {
        
        }
    }

    

    
}
