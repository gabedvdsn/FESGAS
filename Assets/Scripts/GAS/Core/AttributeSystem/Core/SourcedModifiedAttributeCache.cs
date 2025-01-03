using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public class SourcedModifiedAttributeCache
    {
        private const int WAIT_CYCLES = 3;
        
        private Dictionary<AttributeScriptableObject, List<SourcedModifiedAttributeValue>> cache = new();
        private Dictionary<AttributeScriptableObject, int> cycles = new();

        public Dictionary<AttributeScriptableObject, List<SourcedModifiedAttributeValue>>.KeyCollection Attributes => cache.Keys;

        public void Add(AttributeScriptableObject attribute, SourcedModifiedAttributeValue sourcedModifiedValue)
        {
            if (!cache.ContainsKey(attribute))
            {
                cache[attribute] = new List<SourcedModifiedAttributeValue> { sourcedModifiedValue };
            }
            else
            {
                cache[attribute].Add(sourcedModifiedValue);
            }
            foreach (SourcedModifiedAttributeValue smav in cache[attribute]) Debug.Log($"[ SMAC-{attribute.Name} ] {smav}");
        }

        public void Clear()
        {
            List<AttributeScriptableObject> toRemove = new List<AttributeScriptableObject>();
            foreach (AttributeScriptableObject attribute in cycles.Keys)
            {
                if (cache[attribute].Count > 0) cycles[attribute] = 0;
                else if (cycles[attribute] > WAIT_CYCLES) toRemove.Add(attribute);
            }

            foreach (AttributeScriptableObject attribute in toRemove)
            {
                cache.Remove(attribute);
                cycles.Remove(attribute);
            }
        }

        public void Multiply(AttributeScriptableObject attribute, float magnitude)
        {
            if (!cache.ContainsKey(attribute)) return;
            for (int i = 0; i < cache[attribute].Count; i++)
            {
                cache[attribute][i] = cache[attribute][i].Multiply(magnitude);
            }
        }
        
        public void Add(AttributeScriptableObject attribute, float magnitude)
        {
            if (!cache.ContainsKey(attribute)) return;
            for (int i = 0; i < cache[attribute].Count; i++)
            {
                cache[attribute][i] = cache[attribute][i].Add(magnitude);
            }
        }
        
        public void Override(AttributeScriptableObject attribute, float currentMagnitude, float baseMagnitude)
        {
            if (!cache.ContainsKey(attribute)) return;
            for (int i = 0; i < cache[attribute].Count; i++)
            {
                cache[attribute][i] = cache[attribute][i].Override(currentMagnitude, baseMagnitude);
            }
        }

        public void Multiply(AttributeScriptableObject attribute, GameplayEffectShelfContainer container, float magnitude)
        {
            if (!cache.ContainsKey(attribute)) return;
            for (int i = 0; i < cache[attribute].Count; i++)
            {
                if (cache[attribute][i].Container != container) continue;
                cache[attribute][i] = cache[attribute][i].Multiply(magnitude);
            }
        }
        
        public void Add(AttributeScriptableObject attribute, GameplayEffectShelfContainer container, float magnitude)
        {
            if (!cache.ContainsKey(attribute)) return;
            for (int i = 0; i < cache[attribute].Count; i++)
            {
                if (cache[attribute][i].Container != container) continue;
                cache[attribute][i] = cache[attribute][i].Add(magnitude);
            }
        }
        
        public void Override(AttributeScriptableObject attribute, GameplayEffectShelfContainer container, float currentMagnitude, float baseMagnitude)
        {
            if (!cache.ContainsKey(attribute)) return;
            for (int i = 0; i < cache[attribute].Count; i++)
            {
                if (cache[attribute][i].Container != container) continue;
                cache[attribute][i] = cache[attribute][i].Override(currentMagnitude, baseMagnitude);
            }
        }
        
        public void Multiply(AttributeScriptableObject attribute, AbilityScriptableObject ability, float magnitude)
        {
            if (!cache.ContainsKey(attribute)) return;
            for (int i = 0; i < cache[attribute].Count; i++)
            {
                if (cache[attribute][i].Container.Spec.Ability.Base != ability) continue;
                cache[attribute][i] = cache[attribute][i].Multiply(magnitude);
            }
        }
        
        public void Add(AttributeScriptableObject attribute, AbilityScriptableObject ability, float magnitude)
        {
            if (!cache.ContainsKey(attribute)) return;
            for (int i = 0; i < cache[attribute].Count; i++)
            {
                if (cache[attribute][i].Container.Spec.Ability.Base != ability) continue;
                cache[attribute][i] = cache[attribute][i].Add(magnitude);
            }
        }
        
        public void Override(AttributeScriptableObject attribute, AbilityScriptableObject ability, float currentMagnitude, float baseMagnitude)
        {
            if (!cache.ContainsKey(attribute)) return;
            for (int i = 0; i < cache[attribute].Count; i++)
            {
                if (cache[attribute][i].Container.Spec.Ability.Base != ability) continue;
                cache[attribute][i] = cache[attribute][i].Override(currentMagnitude, baseMagnitude);
            }
        }

        public ModifiedAttributeValue ToModified(AttributeScriptableObject attribute)
        {
            TryToModified(attribute, out ModifiedAttributeValue mav);
            return mav;
        }
        public bool TryToModified(AttributeScriptableObject attribute, out ModifiedAttributeValue modifiedAttributeValue)
        {
            if (!TryGetValue(attribute, out var sourcedModifiers))
            {
                modifiedAttributeValue = default;
                return default;
            }
            
            modifiedAttributeValue = sourcedModifiers.Aggregate(new ModifiedAttributeValue(), (current, smav) => current.Combine(smav.ToModifiedAttributeValue()));
            return true;
        }

        public bool TryGetValue(AttributeScriptableObject attribute, out List<SourcedModifiedAttributeValue> sourcedModifiers)
        {
            return cache.TryGetValue(attribute, out sourcedModifiers);
        }

        public bool TryGetValue(AttributeScriptableObject attribute, AbilityScriptableObject source, out List<SourcedModifiedAttributeValue> sourcedModifiers)
        {
            if (!TryGetValue(attribute, out List<SourcedModifiedAttributeValue> foundSMAVs))
            {
                sourcedModifiers = null;
                return false;
            }

            sourcedModifiers = foundSMAVs.Where(smav => smav.Container.Spec.Ability.Base == source).ToList();
            return true;
        }
        
        public bool TryGetValue(AttributeScriptableObject attribute, GASComponent owner, out List<SourcedModifiedAttributeValue> sourcedModifiers)
        {
            if (!TryGetValue(attribute, out List<SourcedModifiedAttributeValue> foundSMAVs))
            {
                sourcedModifiers = null;
                return false;
            }

            sourcedModifiers = foundSMAVs.Where(smav => smav.Container.Spec.Ability.Owner == owner).ToList();
            return true;
        }
    }
    
    
}
