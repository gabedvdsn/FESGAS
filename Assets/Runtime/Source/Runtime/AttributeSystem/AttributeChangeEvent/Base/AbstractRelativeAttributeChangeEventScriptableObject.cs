using System;
using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public abstract class AbstractRelativeAttributeChangeEventScriptableObject : AbstractFocusedAttributeChangeEventScriptableObject
    {
        [Header("Relative Attribute Event")] 
        
        public ESourceTarget From;
        public AttributeScriptableObject RelativeTo;
        public float RelativeMultiplier = 1f;

        public override bool ValidateWorkFor(GASComponentBase system, Dictionary<AttributeScriptableObject, CachedAttributeValue> attributeCache,
            ChangeValue change)
        {
            return From switch
            {

                ESourceTarget.Target => attributeCache.ContainsKey(RelativeTo),
                ESourceTarget.Source => change.Value.BaseDerivation.GetSource().FindAttributeSystem(out var attr) && attr.DefinesAttribute(RelativeTo),
                _ => throw new ArgumentOutOfRangeException()
            } && base.ValidateWorkFor(system, attributeCache, change);
        }

        protected AttributeValue GetRelative(Dictionary<AttributeScriptableObject, CachedAttributeValue> attributeCache, ChangeValue change)
        {
            return From switch
            {

                ESourceTarget.Source => change.Value.BaseDerivation.GetSource().FindAttributeSystem(out var attr) && attr.TryGetAttributeValue(RelativeTo, out AttributeValue value) ? value * RelativeMultiplier : default,
                ESourceTarget.Target => attributeCache[RelativeTo].Value,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
