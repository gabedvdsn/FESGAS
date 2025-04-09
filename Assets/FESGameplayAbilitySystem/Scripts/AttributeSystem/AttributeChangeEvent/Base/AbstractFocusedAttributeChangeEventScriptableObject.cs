using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace FESGameplayAbilitySystem
{
    public abstract class AbstractFocusedAttributeChangeEventScriptableObject : AbstractAttributeChangeEventScriptableObject
    {
        [Header("Focused Change Event")]
        
        public EChangeEventTiming Timing;
        public AttributeScriptableObject TargetAttribute;
        
        [Header("Context Specification")]
        
        public bool AnyContextTag;
        [Tooltip("The modification source context tag(s) (all of them) must exist in this list")]
        public List<GameplayTagScriptableObject> ApplyAbilityContextTags;

        public override bool ValidateWorkFor(GASComponentBase system, ref Dictionary<AttributeScriptableObject, CachedAttributeValue> attributeCache,
            SourcedModifiedAttributeCache modifiedAttributeCache)
        {
            return modifiedAttributeCache.AttributeIsActive(TargetAttribute);
        }

        public override bool RegisterWithHandler(AttributeChangeEventHandler handler)
        {
            return Timing switch
            {
                EChangeEventTiming.PreChange => handler.AddPreChangeEvent(TargetAttribute, this),
                EChangeEventTiming.PostChange => handler.AddPostChangeEvent(TargetAttribute, this),
                EChangeEventTiming.Both => handler.AddPreChangeEvent(TargetAttribute, this) || handler.AddPostChangeEvent(TargetAttribute, this),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public override bool DeRegisterFromHandler(AttributeChangeEventHandler handler)
        {
            return Timing switch
            {

                EChangeEventTiming.PreChange => handler.RemovePreChangeEvent(TargetAttribute, this),
                EChangeEventTiming.PostChange => handler.RemovePostChangeEvent(TargetAttribute, this),
                EChangeEventTiming.Both => handler.RemovePreChangeEvent(TargetAttribute, this) || handler.RemovePostChangeEvent(TargetAttribute, this),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
