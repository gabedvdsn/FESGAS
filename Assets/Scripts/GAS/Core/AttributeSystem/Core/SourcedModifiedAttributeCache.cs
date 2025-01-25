using System;
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
        private List<AttributeScriptableObject> active;

        public Dictionary<AttributeScriptableObject, List<SourcedModifiedAttributeValue>>.KeyCollection Attributes => cache.Keys;

        public void SubscribeAttribute(AttributeScriptableObject attribute)
        {
            cache[attribute] = new List<SourcedModifiedAttributeValue>();
        }
        
        public void Add(AttributeScriptableObject attribute, SourcedModifiedAttributeValue sourcedModifiedValue)
        {
            if (!cache.ContainsKey(attribute)) return;
            
            cache[attribute].Add(sourcedModifiedValue);
            // foreach (SourcedModifiedAttributeValue smav in cache[attribute]) Debug.Log($"[ SMAC-{attribute.Name} ] {smav}");
        }

        public List<AttributeScriptableObject> Get() => active;

        public void Clear()
        {
            if (active.Count == 0) return;
            foreach (AttributeScriptableObject attribute in active) cache[attribute].Clear();
        }

        public void Set(AttributeScriptableObject attribute, ModifiedAttributeValue modifiedAttributeValue)
        {
            if (!cache.ContainsKey(attribute)) return;
        }

        public void LimitTop(AttributeScriptableObject attribute, ModifiedAttributeValue limit,
            AttributeModificationApplicationMethod method = AttributeModificationApplicationMethod.FromLast)
        {
            if (!cache.ContainsKey(attribute)) return;

            ModifiedAttributeValue mav = ToModified(attribute);
            if (mav.DeltaCurrentValue <= limit.DeltaCurrentValue && mav.DeltaBaseValue <= limit.DeltaBaseValue) return;
            
            ModifiedAttributeValue remaining = default;
            if (mav.DeltaCurrentValue > limit.DeltaCurrentValue)
                remaining.DeltaCurrentValue = mav.DeltaCurrentValue - limit.DeltaCurrentValue;
            if (mav.DeltaBaseValue > limit.DeltaBaseValue)
                remaining.DeltaBaseValue = mav.DeltaBaseValue - limit.DeltaBaseValue;
            
            switch (method)
            {
                case AttributeModificationApplicationMethod.FromLast:
                    int index = cache[attribute].Count - 1;
                    while (index >= 0)
                    {
                        if (remaining.DeltaCurrentValue > 0) cache[attribute][index] = cache[attribute][index].DeltaCurrentValue
                        index -= 1;
                    }
                    
                    break;
                case AttributeModificationApplicationMethod.FromFirst:
                    break;
                case AttributeModificationApplicationMethod.FromEach:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(method), method, null);
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
                return false;
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

    public enum AttributeModificationApplicationMethod
    {
        FromLast,
        FromFirst,
        FromEach
    }
    
}
