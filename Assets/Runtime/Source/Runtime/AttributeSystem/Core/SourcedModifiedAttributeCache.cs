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
        
        #region Core
        
        public void SubscribeModifiableAttribute(AttributeScriptableObject attribute)
        {
            cache[attribute] = new List<SourcedModifiedAttributeValue>();
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
            if (impactType == EImpactType.NotApplicable) return false;
            return impactType == validateAgainst;
        }
        
        public void Register(AttributeScriptableObject attribute, SourcedModifiedAttributeValue sourcedModifiedValue)
        {
            if (!ValidateAttribute(attribute)) return;
            
            cache[attribute].Add(sourcedModifiedValue);
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
        
        #endregion
        
        public void Multiply(AttributeScriptableObject attribute, AttributeValue operand, bool allowSelfModify, List<GameplayTagScriptableObject> contextTags, bool anyContextTag)
        {
            if (!ValidateAttribute(attribute)) return;
            
            for (int i = 0; i < cache[attribute].Count; i++)
            {
                if (cache[attribute][i].BaseDerivation.GetSource() == cache[attribute][i].BaseDerivation.GetTarget() && !allowSelfModify) continue;
                if (!anyContextTag && !cache[attribute][i].BaseDerivation.GetContextTags().ContainsAll(contextTags)) continue;
                cache[attribute][i] = cache[attribute][i].Multiply(operand);
            }
        }
        
        public void Multiply(AttributeScriptableObject attribute, ESignPolicy signPolicy, AttributeValue operand, bool allowSelfModify, List<GameplayTagScriptableObject> contextTags, bool anyContextTag)
        {
            if (!ValidateAttribute(attribute)) return;
            
            for (int i = 0; i < cache[attribute].Count; i++)
            {
                if (cache[attribute][i].SignPolicy != signPolicy) continue;
                if (cache[attribute][i].BaseDerivation.GetSource() == cache[attribute][i].BaseDerivation.GetTarget() && !allowSelfModify) continue;
                if (!anyContextTag && !cache[attribute][i].BaseDerivation.GetContextTags().ContainsAll(contextTags)) continue;
                cache[attribute][i] = cache[attribute][i].Multiply(operand);
            }
        }

        public void MultiplyAmplify(AttributeScriptableObject attribute, EImpactType impactType, ESignPolicy signPolicy, AttributeValue operand, bool allowSelfModify, List<GameplayTagScriptableObject> contextTags, bool anyContextTag)
        {
            if (!ValidateAttribute(attribute)) return;
            
            for (int i = 0; i < cache[attribute].Count; i++)
            {
                if (!ValidateImpactType(impactType, cache[attribute][i].Derivation.GetImpactType())) continue;
                if (cache[attribute][i].SignPolicy != signPolicy) continue;
                if (cache[attribute][i].BaseDerivation.GetSource() == cache[attribute][i].BaseDerivation.GetTarget() && !allowSelfModify) continue;
                if (!anyContextTag && !cache[attribute][i].BaseDerivation.GetContextTags().ContainsAll(contextTags)) continue;
                cache[attribute][i] = cache[attribute][i].Multiply(1 + operand);
            }
        }

        public void MultiplyAttenuate(AttributeScriptableObject attribute, EImpactType impactType, ESignPolicy signPolicy, AttributeValue operand, bool allowSelfModify, List<GameplayTagScriptableObject> contextTags, bool anyContextTag)
        {
            if (!ValidateAttribute(attribute)) return;
            
            for (int i = 0; i < cache[attribute].Count; i++)
            {
                if (!ValidateImpactType(impactType, cache[attribute][i].Derivation.GetImpactType())) continue;
                if (cache[attribute][i].SignPolicy != signPolicy) continue;
                if (cache[attribute][i].BaseDerivation.GetSource() == cache[attribute][i].BaseDerivation.GetTarget() && !allowSelfModify) continue;
                if (!anyContextTag && !cache[attribute][i].BaseDerivation.GetContextTags().ContainsAll(contextTags)) continue;
                cache[attribute][i] = cache[attribute][i].Multiply(1 - operand);
            }
        }
        
        public void Add(AttributeScriptableObject attribute, AttributeValue operand, bool allowSelfModify, List<GameplayTagScriptableObject> contextTags, bool anyContextTag, bool spread = false)
        {
            if (!ValidateAttribute(attribute)) return;

            AttributeValue addValue = spread ? operand : operand / cache[attribute].Count;
            for (int i = 0; i < cache[attribute].Count; i++)
            {
                if (cache[attribute][i].BaseDerivation.GetSource() == cache[attribute][i].BaseDerivation.GetTarget() && !allowSelfModify) continue;
                if (!anyContextTag && !cache[attribute][i].BaseDerivation.GetContextTags().ContainsAll(contextTags)) continue;
                cache[attribute][i] = cache[attribute][i].Add(addValue);
            }
        }

        public void Add(AttributeScriptableObject attribute, ESignPolicy signPolicy, AttributeValue operand, bool allowSelfModify, List<GameplayTagScriptableObject> contextTags, bool anyContextTag, bool spread = false)
        {
            if (!ValidateAttribute(attribute)) return;
            
            AttributeValue addValue = spread ? operand : operand / cache[attribute].Count;
            for (int i = 0; i < cache[attribute].Count; i++)
            {
                if (cache[attribute][i].SignPolicy != signPolicy) continue;
                if (cache[attribute][i].BaseDerivation.GetSource() == cache[attribute][i].BaseDerivation.GetTarget() && !allowSelfModify) continue;
                if (!anyContextTag && !cache[attribute][i].BaseDerivation.GetContextTags().ContainsAll(contextTags)) continue;
                cache[attribute][i] = cache[attribute][i].Add(addValue);
            }
        }
        
        public void Override(AttributeScriptableObject attribute, AttributeValue operand, bool allowSelfModify, List<GameplayTagScriptableObject> contextTags, bool anyContextTag)
        {
            if (!ValidateAttribute(attribute)) return;
            
            for (int i = 0; i < cache[attribute].Count; i++)
            {
                if (cache[attribute][i].BaseDerivation.GetSource() == cache[attribute][i].BaseDerivation.GetTarget() && !allowSelfModify) continue;
                if (!anyContextTag && !cache[attribute][i].BaseDerivation.GetContextTags().ContainsAll(contextTags)) continue;
                cache[attribute][i] = cache[attribute][i].Override(operand);
            }
        }
        
        public void Override(AttributeScriptableObject attribute, ESignPolicy signPolicy, AttributeValue operand, bool allowSelfModify, List<GameplayTagScriptableObject> contextTags, bool anyContextTag)
        {
            if (!ValidateAttribute(attribute)) return;
            
            for (int i = 0; i < cache[attribute].Count; i++)
            {
                if (cache[attribute][i].SignPolicy != signPolicy) continue;
                if (cache[attribute][i].BaseDerivation.GetSource() == cache[attribute][i].BaseDerivation.GetTarget() && !allowSelfModify) continue;
                if (!anyContextTag && !cache[attribute][i].BaseDerivation.GetContextTags().ContainsAll(contextTags)) continue;
                cache[attribute][i] = cache[attribute][i].Override(operand);
            }
        }
    }

    public enum EAttributeModificationMethod
    {
        FromLast,
        FromFirst
    }
    
}
