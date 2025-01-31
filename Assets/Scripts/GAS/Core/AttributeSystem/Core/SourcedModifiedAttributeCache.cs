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

        /// <summary>
        /// Ceils the total MAV values for the specified attribute by the provided limit.
        /// </summary>
        /// <param name="attribute">The attribute to limit</param>
        /// <param name="limit">The limit value</param>
        /// <param name="method">The application method</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /*public void Ceil(AttributeScriptableObject attribute, ModifiedAttributeValue limit,
            AttributeModificationApplicationMethod method = AttributeModificationApplicationMethod.FromLast)
        {
            if (!TryToModified(attribute, out ModifiedAttributeValue mavSum)) return;
            
            float totalCurr = mavSum.DeltaCurrentValue;
            float totalBase = mavSum.DeltaBaseValue;

            float excessCurr = Mathf.Max(0f, totalCurr - limit.DeltaCurrentValue);
            float excessBase = Mathf.Max(0f, totalBase - limit.DeltaBaseValue);

            switch (method)
            {

                case AttributeModificationApplicationMethod.FromLast:
                    for (int i = cache[attribute].Count - 1; i >= 0; i--)
                    {
                        if (excessCurr == 0f && excessBase == 0f) break;
                        ModifiedAttributeValue newMav = cache[attribute][i].ToModified();
                        
                        if (excessCurr > 0)
                        {
                            float reduction = Mathf.Min(cache[attribute][i].DeltaCurrentValue, excessCurr);
                            newMav.DeltaCurrentValue -= reduction;
                            excessCurr -= reduction;
                        }
                        
                        if (excessBase > 0)
                        {
                            float reduction = Mathf.Min(cache[attribute][i].DeltaBaseValue, excessBase);
                            newMav.DeltaBaseValue -= reduction;
                            excessBase -= reduction;
                        }

                        cache[attribute][i] = newMav.ToSourced(cache[attribute][i].SourceSpec);
                    }
                    break;
                case AttributeModificationApplicationMethod.FromFirst:
                    for (int i = 0; i < cache[attribute].Count; i++)
                    {
                        if (excessCurr == 0f && excessBase == 0f) break;
                        ModifiedAttributeValue newMav = cache[attribute][i].ToModified();
                        
                        if (excessCurr > 0)
                        {
                            float reduction = Mathf.Min(cache[attribute][i].DeltaCurrentValue, excessCurr);
                            newMav.DeltaCurrentValue -= reduction;
                            excessCurr -= reduction;
                        }
                        
                        if (excessBase > 0)
                        {
                            float reduction = Mathf.Min(cache[attribute][i].DeltaBaseValue, excessBase);
                            newMav.DeltaBaseValue -= reduction;
                            excessBase -= reduction;
                        }

                        cache[attribute][i] = newMav.ToSourced(cache[attribute][i].SourceSpec);
                    }
                    break;
                case AttributeModificationApplicationMethod.FromEach:
                    while (excessCurr > 0 || excessBase > 0)
                    {
                        for (int i = 0; i < cache[attribute].Count; i++)
                        {
                            if (excessCurr == 0f && excessBase == 0f) break;
                            ModifiedAttributeValue newMav = cache[attribute][i].ToModified();
                        
                            if (excessCurr > 0)
                            {
                                float reduction = Mathf.Min(cache[attribute][i].DeltaCurrentValue, excessCurr, Mathf.Max(1f, Mathf.Floor(excessCurr / cache[attribute].Count)));
                                newMav.DeltaCurrentValue -= reduction;
                                excessCurr -= reduction;
                            }
                        
                            if (excessBase > 0)
                            {
                                float reduction = Mathf.Min(cache[attribute][i].DeltaBaseValue, excessBase, Mathf.Max(1f, Mathf.Floor(excessBase / cache[attribute].Count)));
                                newMav.DeltaBaseValue -= reduction;
                                excessBase -= reduction;
                            }

                            cache[attribute][i] = newMav.ToSourced(cache[attribute][i].SourceSpec);
                        }
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(method), method, null);
            }
        }

        public void Floor(AttributeScriptableObject attribute, ModifiedAttributeValue limit,
            AttributeModificationApplicationMethod method = AttributeModificationApplicationMethod.FromLast)
        {
            if (!TryToModified(attribute, out ModifiedAttributeValue mavSum)) return;
            
            float totalCurr = mavSum.DeltaCurrentValue;
            float totalBase = mavSum.DeltaBaseValue;

            float deficitCurr = Mathf.Max(0f, limit.DeltaCurrentValue - totalCurr);
            float deficitBase = Mathf.Max(0f, limit.DeltaBaseValue - totalBase);
        }*/

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
        
        public void Multiply(AttributeScriptableObject attribute, EDamageType damageType, SignPolicy signPolicy, float multiplier, bool isScalar, bool clampScalar)
        {
            if (!cache.ContainsKey(attribute)) return;
            for (int i = 0; i < cache[attribute].Count; i++)
            {
                if (cache[attribute][i].SourceSpec.Base.ImpactSpecification.ImpactType != EDamageType.NotApplicable 
                    && cache[attribute][i].SourceSpec.Base.ImpactSpecification.ImpactType != damageType) continue;
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
        
        public bool TryGetCachedValue(AttributeScriptableObject attribute, AbilityScriptableObject source, out List<SourcedModifiedAttributeValue> sourcedModifiers)
        {
            if (!TryGetCachedValue(attribute, out List<SourcedModifiedAttributeValue> foundSMAVs))
            {
                sourcedModifiers = null;
                return false;
            }

            sourcedModifiers = foundSMAVs.Where(smav => smav.SourceSpec.Ability.Base == source).ToList();
            return true;
        }
        
        public bool TryGetCachedValue(AttributeScriptableObject attribute, GASComponent owner, out List<SourcedModifiedAttributeValue> sourcedModifiers)
        {
            if (!TryGetCachedValue(attribute, out List<SourcedModifiedAttributeValue> foundSMAVs))
            {
                sourcedModifiers = null;
                return false;
            }

            sourcedModifiers = foundSMAVs.Where(smav => smav.SourceSpec.Ability.Owner == owner).ToList();
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
