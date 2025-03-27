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
        
        public void SubscribeModifiableAttribute(AttributeScriptableObject attribute)
        {
            cache[attribute] = new List<SourcedModifiedAttributeValue>();
            Debug.Log($"Subscribed attribute: {attribute}");
        }

        public bool AttributeIsActive(AttributeScriptableObject attribute) => cache.ContainsKey(attribute) && cache[attribute].Count > 0;
        public bool DefinesAttribute(AttributeScriptableObject attribute) => cache.ContainsKey(attribute);

        public List<AttributeScriptableObject> GetDefined() => cache.Keys.ToList();
        public List<AttributeScriptableObject> GetModified() => active;

        public void Clear()
        {
            if (active.Count == 0) return;
            foreach (AttributeScriptableObject attribute in active) cache[attribute].Clear();
            active.Clear();
        }

        private bool ValidateAttribute(AttributeScriptableObject attribute)
        {
            if (!cache.ContainsKey(attribute)) return false;
            if (!active.Contains(attribute)) active.Add(attribute);

            return true;
        }

        private bool ValidateImpactType(EImpactType impactType, EImpactType validateAgainst)
        {
            if (impactType is EImpactType.NotApplicable or EImpactType.Pure) return false;
            return impactType == validateAgainst;
        }
        
        public void Register(AttributeScriptableObject attribute, SourcedModifiedAttributeValue sourcedModifiedValue)
        {
            if (!ValidateAttribute(attribute)) return;
            
            cache[attribute].Add(sourcedModifiedValue);
        }
        
        public void Multiply(AttributeScriptableObject attribute, AttributeValue attributeValue)
        {
            if (!ValidateAttribute(attribute)) return;
            
            for (int i = 0; i < cache[attribute].Count; i++)
            {
                cache[attribute][i] = cache[attribute][i].Multiply(attributeValue);
            }
        }
        
        public void Multiply(AttributeScriptableObject attribute, ESignPolicy signPolicy, AttributeValue multiplier)
        {
            if (!ValidateAttribute(attribute)) return;
            
            for (int i = 0; i < cache[attribute].Count; i++)
            {
                if (cache[attribute][i].SignPolicy != signPolicy) continue;
                cache[attribute][i] = cache[attribute][i].Multiply(multiplier);
            }
        }

        public void MultiplyAmplify(AttributeScriptableObject attribute, EImpactType impactType, ESignPolicy signPolicy, AttributeValue multiplier)
        {
            if (!ValidateAttribute(attribute)) return;
            
            for (int i = 0; i < cache[attribute].Count; i++)
            {
                if (!ValidateImpactType(impactType, cache[attribute][i].Derivation.GetImpactType())) continue;
                if (cache[attribute][i].SignPolicy != signPolicy) continue;
                cache[attribute][i] = cache[attribute][i].Multiply(1 + multiplier);
            }
        }

        public void MultiplyAttenuate(AttributeScriptableObject attribute, EImpactType impactType, ESignPolicy signPolicy, AttributeValue multiplier)
        {
            if (!ValidateAttribute(attribute)) return;
            
            for (int i = 0; i < cache[attribute].Count; i++)
            {
                if (!ValidateImpactType(impactType, cache[attribute][i].Derivation.GetImpactType())) continue;
                if (cache[attribute][i].SignPolicy != signPolicy) continue;
                cache[attribute][i] = cache[attribute][i].Multiply(1 - multiplier);
            }
        }
        
        public void Add(AttributeScriptableObject attribute, ModifiedAttributeValue modifiedAttributeValue, bool spread = false)
        {
            if (!ValidateAttribute(attribute)) return;

            ModifiedAttributeValue addValue = spread ? modifiedAttributeValue : modifiedAttributeValue / cache[attribute].Count;
            for (int i = 0; i < cache[attribute].Count; i++)
            {
                cache[attribute][i] = cache[attribute][i].Add(addValue);
            }
        }
        
        public void Override(AttributeScriptableObject attribute, ModifiedAttributeValue modifiedAttributeValue)
        {
            if (!ValidateAttribute(attribute)) return;
            
            for (int i = 0; i < cache[attribute].Count; i++)
            {
                cache[attribute][i] = cache[attribute][i].Override(modifiedAttributeValue);
            }
        }
        
        public bool TryToModified(AttributeScriptableObject attribute, out ModifiedAttributeValue modifiedAttributeValue)
        {
            if (!active.Contains(attribute) || !TryGetSourcedModifiers(attribute, out var sourcedModifiers))
            {
                modifiedAttributeValue = default;
                return false;
            }
            
            modifiedAttributeValue = sourcedModifiers.Aggregate(new ModifiedAttributeValue(), (current, smav) => current.Combine(smav.ToModified()));
            return true;
        }

        public bool TryGetSourcedModifiers(AttributeScriptableObject attribute, out List<SourcedModifiedAttributeValue> sourcedModifiers)
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
