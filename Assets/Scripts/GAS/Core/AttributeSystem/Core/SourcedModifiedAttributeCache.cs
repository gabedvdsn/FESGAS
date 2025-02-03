using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public class SourcedModifiedAttributeCache
    {
        private Dictionary<AttributeScriptableObject, List<SourcedModifiedAttributeValue>> cache = new();
        private List<AttributeScriptableObject> active = new();

        public Dictionary<AttributeScriptableObject, List<SourcedModifiedAttributeValue>>.KeyCollection Attributes => cache.Keys;
        
        public void SubscribeAttribute(AttributeScriptableObject attribute)
        {
            cache[attribute] = new List<SourcedModifiedAttributeValue>();
            Debug.Log($"Subscribed attribute: {attribute}");
        }

        public bool DefinesAttribute(AttributeScriptableObject attribute) => cache.ContainsKey(attribute);
        
        public void Add(AttributeScriptableObject attribute, SourcedModifiedAttributeValue sourcedModifiedValue)
        {
            if (!cache.ContainsKey(attribute)) return;
            if (!active.Contains(attribute)) active.Add(attribute);
            
            cache[attribute].Add(sourcedModifiedValue);
            //foreach (SourcedModifiedAttributeValue smav in cache[attribute]) Debug.Log($"[ SMAC-{attribute} ] {smav}");
        }

        public List<AttributeScriptableObject> Get() => active;

        public void Clear()
        {
            if (active.Count == 0) return;
            foreach (AttributeScriptableObject attribute in active) cache[attribute].Clear();
            active.Clear();
        }
        
        public void Multiply(AttributeScriptableObject attribute, ModifiedAttributeValue modifiedAttributeValue)
        {
            if (!cache.ContainsKey(attribute)) return;
            for (int i = 0; i < cache[attribute].Count; i++)
            {
                cache[attribute][i] = cache[attribute][i].Multiply(modifiedAttributeValue);
            }
        }
        
        public void Multiply(AttributeScriptableObject attribute, AttributeValue attributeValue)
        {
            if (!cache.ContainsKey(attribute)) return;
            for (int i = 0; i < cache[attribute].Count; i++)
            {
                cache[attribute][i] = cache[attribute][i].Multiply(attributeValue);
            }
        }

        public void Multiply(AttributeScriptableObject attribute, SignPolicy signPolicy, float multiplier, bool isScalar, bool clampScalar)
        {
            if (!cache.ContainsKey(attribute)) return;
            for (int i = 0; i < cache[attribute].Count; i++)
            {
                if (cache[attribute][i].SignPolicy != signPolicy) continue;
                if (isScalar)
                {
                    if (clampScalar) cache[attribute][i] = cache[attribute][i].Multiply(Mathf.Clamp01(1 - multiplier));
                    else cache[attribute][i] = cache[attribute][i].Multiply(1 - multiplier);
                }
                else cache[attribute][i] = cache[attribute][i].Multiply(multiplier);
            }
        }
        
        public void Multiply(AttributeScriptableObject attribute, EImpactType impactType, SignPolicy signPolicy, float multiplier, bool isScalar, bool clampScalar)
        {
            if (!cache.ContainsKey(attribute)) return;
            for (int i = 0; i < cache[attribute].Count; i++)
            {
                if (cache[attribute][i].Derivation.GetImpactType() != EImpactType.NotApplicable 
                    && cache[attribute][i].Derivation.GetImpactType() != impactType) continue;
                if (cache[attribute][i].SignPolicy != signPolicy) continue;
                if (isScalar)
                {
                    if (clampScalar) cache[attribute][i] = cache[attribute][i].Multiply(Mathf.Clamp01(1 - multiplier));
                    else cache[attribute][i] = cache[attribute][i].Multiply(1 - multiplier);
                }
                else cache[attribute][i] = cache[attribute][i].Multiply(multiplier);
            }
        }
        
        public void Add(AttributeScriptableObject attribute, ModifiedAttributeValue modifiedAttributeValue)
        {
            if (!cache.ContainsKey(attribute)) return;
            for (int i = 0; i < cache[attribute].Count; i++)
            {
                cache[attribute][i] = cache[attribute][i].Add(modifiedAttributeValue);
            }
        }
        
        public void Override(AttributeScriptableObject attribute, ModifiedAttributeValue modifiedAttributeValue)
        {
            if (!cache.ContainsKey(attribute)) return;
            for (int i = 0; i < cache[attribute].Count; i++)
            {
                cache[attribute][i] = cache[attribute][i].Override(modifiedAttributeValue);
            }
        }

        public bool AttributeIsActive(AttributeScriptableObject attribute) => cache.ContainsKey(attribute) && cache[attribute].Count > 0;
        
        public ModifiedAttributeValue ToModified(AttributeScriptableObject attribute)
        {
            TryToModified(attribute, out ModifiedAttributeValue mav);
            return mav;
        }
        
        public bool TryToModified(AttributeScriptableObject attribute, out ModifiedAttributeValue modifiedAttributeValue)
        {
            if (!active.Contains(attribute) || !TryGetCachedValue(attribute, out var sourcedModifiers))
            {
                modifiedAttributeValue = default;
                return false;
            }
            
            modifiedAttributeValue = sourcedModifiers.Aggregate(new ModifiedAttributeValue(), (current, smav) => current.Combine(smav.ToModified()));
            return true;
        }

        public bool TryGetCachedValue(AttributeScriptableObject attribute, out List<SourcedModifiedAttributeValue> sourcedModifiers)
        {
            return cache.TryGetValue(attribute, out sourcedModifiers);
        }
    }

    public enum EAttributeModificationMethod
    {
        FromLast,
        FromFirst
    }
    
}
