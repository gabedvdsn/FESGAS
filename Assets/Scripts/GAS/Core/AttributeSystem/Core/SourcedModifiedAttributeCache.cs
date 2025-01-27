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
        private List<AttributeScriptableObject> active;

        public Dictionary<AttributeScriptableObject, List<SourcedModifiedAttributeValue>>.KeyCollection Attributes => cache.Keys;

        
        
        public void SubscribeAttribute(AttributeScriptableObject attribute)
        {
            cache[attribute] = new List<SourcedModifiedAttributeValue>();
        }

        public bool DefinesAttribute(AttributeScriptableObject attribute) => cache.ContainsKey(attribute);
        
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
            active.Clear();
        }

        public void Set(AttributeScriptableObject attribute, ModifiedAttributeValue modifiedAttributeValue)
        {
            if (!DefinesAttribute(attribute)) return;
            
        }

        /// <summary>
        /// Ceils the total MAV values for the specified attribute by the provided limit.
        /// </summary>
        /// <param name="attribute">The attribute to limit</param>
        /// <param name="limit">The limit value</param>
        /// <param name="method">The application method</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void Ceil(AttributeScriptableObject attribute, ModifiedAttributeValue limit,
            AttributeModificationApplicationMethod method = AttributeModificationApplicationMethod.FromLast)
        {
            if (!TryToModified(attribute, out ModifiedAttributeValue mav)) return;

            if (mav.DeltaCurrentValue <= limit.DeltaCurrentValue && mav.DeltaBaseValue <= limit.DeltaBaseValue) return;
            
            ModifiedAttributeValue remaining = default;
            if (mav.DeltaCurrentValue > limit.DeltaCurrentValue)
                remaining.DeltaCurrentValue = mav.DeltaCurrentValue - limit.DeltaCurrentValue;
            if (mav.DeltaBaseValue > limit.DeltaBaseValue)
                remaining.DeltaBaseValue = mav.DeltaBaseValue - limit.DeltaBaseValue;

            int cacheIndex;
            switch (method)
            {
                case AttributeModificationApplicationMethod.FromLast:
                    cacheIndex = cache[attribute].Count - 1;
                    while (cacheIndex >= 0)
                    {
                        if (!(remaining.DeltaCurrentValue > 0 && remaining.DeltaBaseValue > 0)) break;

                        ModifiedAttributeValue newMav = Limit(attribute, cacheIndex, remaining, 0, float.MaxValue);

                        remaining -= cache[attribute][cacheIndex].ToModified() - newMav;
                        cache[attribute][cacheIndex] = newMav.ToSourced(cache[attribute][cacheIndex].Container);
                        
                        cacheIndex -= 1;
                    }
                    
                    break;
                case AttributeModificationApplicationMethod.FromFirst:
                    cacheIndex = 0;
                    while (cacheIndex < cache[attribute].Count)
                    {
                        if (!(remaining.DeltaCurrentValue > 0 && remaining.DeltaBaseValue > 0)) break;

                        ModifiedAttributeValue newMav = Limit(attribute, cacheIndex, remaining, 0, float.MaxValue);

                        remaining -= cache[attribute][cacheIndex].ToModified() - newMav;
                        cache[attribute][cacheIndex] = newMav.ToSourced(cache[attribute][cacheIndex].Container);
                        
                        cacheIndex += 1;
                    }
                    break;
                case AttributeModificationApplicationMethod.FromEach:
                    
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(method), method, null);
            }
        }

        private ModifiedAttributeValue Limit(AttributeScriptableObject attribute, int cacheIndex, ModifiedAttributeValue remaining, float clampMin, float clampMax)
        {
            ModifiedAttributeValue newMAV = default;
            newMAV.DeltaCurrentValue = Mathf.Clamp(cache[attribute][cacheIndex].DeltaCurrentValue - remaining.DeltaCurrentValue, clampMin, clampMax);
            newMAV.DeltaBaseValue = Mathf.Clamp(cache[attribute][cacheIndex].DeltaBaseValue - remaining.DeltaBaseValue, clampMin, clampMax);

            return newMAV;
        }

        public void Multiply(AttributeScriptableObject attribute, ModifiedAttributeValue modifiedAttributeValue)
        {
            if (!cache.ContainsKey(attribute)) return;
            for (int i = 0; i < cache[attribute].Count; i++)
            {
                cache[attribute][i] = cache[attribute][i].Multiply(modifiedAttributeValue);
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
                cache[attribute][i] = cache[attribute][i].Override(currentMagnitude, baseMagnitude);
            }
        }

        public void Multiply(AttributeScriptableObject attribute, GameplayEffectShelfContainer container, ModifiedAttributeValue modifiedAttributeValue)
        {
            if (!cache.ContainsKey(attribute)) return;
            for (int i = 0; i < cache[attribute].Count; i++)
            {
                if (cache[attribute][i].Container != container) continue;
                cache[attribute][i] = cache[attribute][i].Multiply(magnitude);
            }
        }
        
        public void Add(AttributeScriptableObject attribute, GameplayEffectShelfContainer container, ModifiedAttributeValue modifiedAttributeValue)
        {
            if (!cache.ContainsKey(attribute)) return;
            for (int i = 0; i < cache[attribute].Count; i++)
            {
                if (cache[attribute][i].Container != container) continue;
                cache[attribute][i] = cache[attribute][i].Add(magnitude);
            }
        }
        
        public void Override(AttributeScriptableObject attribute, GameplayEffectShelfContainer container, ModifiedAttributeValue modifiedAttributeValue)
        {
            if (!cache.ContainsKey(attribute)) return;
            for (int i = 0; i < cache[attribute].Count; i++)
            {
                if (cache[attribute][i].Container != container) continue;
                cache[attribute][i] = cache[attribute][i].Override(currentMagnitude, baseMagnitude);
            }
        }
        
        public void Multiply(AttributeScriptableObject attribute, AbilityScriptableObject ability, ModifiedAttributeValue modifiedAttributeValue)
        {
            if (!cache.ContainsKey(attribute)) return;
            for (int i = 0; i < cache[attribute].Count; i++)
            {
                if (cache[attribute][i].Container.Spec.Ability.Base != ability) continue;
                cache[attribute][i] = cache[attribute][i].Multiply(magnitude);
            }
        }
        
        public void Add(AttributeScriptableObject attribute, AbilityScriptableObject ability, ModifiedAttributeValue modifiedAttributeValue)
        {
            if (!cache.ContainsKey(attribute)) return;
            for (int i = 0; i < cache[attribute].Count; i++)
            {
                if (cache[attribute][i].Container.Spec.Ability.Base != ability) continue;
                cache[attribute][i] = cache[attribute][i].Add(magnitude);
            }
        }
        
        public void Override(AttributeScriptableObject attribute, AbilityScriptableObject ability, ModifiedAttributeValue modifiedAttributeValue)
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
        
        public bool TryGetCachedValue(AttributeScriptableObject attribute, AbilityScriptableObject source, out List<SourcedModifiedAttributeValue> sourcedModifiers)
        {
            if (!TryGetCachedValue(attribute, out List<SourcedModifiedAttributeValue> foundSMAVs))
            {
                sourcedModifiers = null;
                return false;
            }

            sourcedModifiers = foundSMAVs.Where(smav => smav.Container.Spec.Ability.Base == source).ToList();
            return true;
        }
        
        public bool TryGetCachedValue(AttributeScriptableObject attribute, GASComponent owner, out List<SourcedModifiedAttributeValue> sourcedModifiers)
        {
            if (!TryGetCachedValue(attribute, out List<SourcedModifiedAttributeValue> foundSMAVs))
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
