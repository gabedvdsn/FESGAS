using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public static class FESGAS
    {
        public static class Modifiers
        {
            #region Normal
            
            public static AbstractMagnitudeModifier CreateSimpleModifier(AnimationCurve arg)
            {
                var value = new SimpleMagnitudeModifier();
                value.Scaling = arg;
                return value;
            }
            
            public static AbstractMagnitudeModifier CreateAttributeBackedModifier(AnimationCurve arg, EEffectImpactTargetLimited scalingPolicy,
                Attribute captureAttribute)
            {
                var value = new SimpleMagnitudeModifier();
                value.Scaling = arg;
                return value;
            }
            
            public static AbstractMagnitudeModifier CreateModifierGroup(MagnitudeModifierGroupMember[] arg, EValueCollisionPolicy collisionPolicy)
            {
                var value = new MagnitudeModifierGroup();
                value.Calculations = arg;
                value.OverrideMemberCollisionPolicy = collisionPolicy;
                return value;
            }
            
            #endregion
            
            #region Cached Attribute

            public static AbstractCachedMagnitudeModifier CreateCachedSimpleModifier(AnimationCurve arg)
            {
                var value = new AttrCacheSimpleMagnitudeModifier();
                value.Scaling = arg;
                return value;
            }

            public static AbstractCachedMagnitudeModifier CreateCachedAttributeBackedModifier(AnimationCurve arg, EEffectImpactTargetLimited scalingPolicy, Attribute attribute)
            {
                var value = new AttrCacheBackedMagnitudeModifier();
                value.Scaling = arg;
                value.ScalingPolicy = scalingPolicy;
                value.CaptureAttribute = attribute;
                return value;
            }

            public static AbstractCachedMagnitudeModifier CreateCachedModifierGroup(CachedMagnitudeModifierGroupMember[] arg, EValueCollisionPolicy collisionPolicy)
            {
                var value = new CachedMagnitudeModifierGroup();
                value.Calculations = arg;
                value.OverrideMemberCollisionPolicy = collisionPolicy;
                return value;
            }
            
            #endregion
        }

        public static class Effect
        {
            public static GameplayEffect CreateGameplayEffect()
            {
                
            }
        }
    }

    public static class Effects
    {
        
    }

    public static class Requirements
    {
        public static TagRequirements CreateEffectRequirement(
            AvoidRequireTagGroup application,
            AvoidRequireTagGroup ongoing,
            AvoidRequireTagGroup removal,
            TagRequirements[] nested
        )
        {
            var value = new TagRequirements();
            value.ApplicationRequirements = application;
            value.OngoingRequirements = ongoing;
            value.RemovalRequirements = removal;
            value.NestedRequirements = nested;
            return value;
        }
        
        public static TagRequirements CreateEmptyRequirements()
        {
            return new TagRequirements();
        }
    }

    public static class Abilities
    {
        
    }

    public static class Workers
    {
        
    }
}
