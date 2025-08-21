using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public class SourcedModifiedAttributeCache
    {
        private Dictionary<IAttribute, List<SourcedModifiedAttributeValue>> cache = new();
        private List<IAttribute> active = new();

        public Dictionary<IAttribute, List<SourcedModifiedAttributeValue>>.KeyCollection Attributes => cache.Keys;
        
        #region Core
        
        public void SubscribeModifiableAttribute(IAttribute attribute)
        {
            cache[attribute] = new List<SourcedModifiedAttributeValue>();
        }

        public bool AttributeIsActive(IAttribute attribute) => cache.ContainsKey(attribute) && cache[attribute].Count > 0;
        public bool DefinesAttribute(IAttribute attribute) => cache.ContainsKey(attribute);

        public List<IAttribute> GetDefined() => cache.Keys.ToList();
        public List<IAttribute> GetModified() => active;

        public void Clear()
        {
            if (active.Count == 0) return;
            foreach (IAttribute attribute in active) cache[attribute].Clear();
            active.Clear();
        }

        private bool ValidateAttribute(IAttribute attribute)
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
        
        public void Register(IAttribute attribute, SourcedModifiedAttributeValue sourcedModifiedValue)
        {
            if (!ValidateAttribute(attribute)) return;
            
            cache[attribute].Add(sourcedModifiedValue);
        }
        
        public bool TryToModified(IAttribute attribute, out ModifiedAttributeValue modifiedAttributeValue)
        {
            if (!active.Contains(attribute) || !TryGetSourcedModifiers(attribute, out var sourcedModifiers))
            {
                modifiedAttributeValue = default;
                return false;
            }
            
            modifiedAttributeValue = sourcedModifiers.Aggregate(new ModifiedAttributeValue(), (current, smav) => current.Combine(smav.ToModified()));
            return true;
        }

        public bool TryGetSourcedModifiers(IAttribute attribute, out List<SourcedModifiedAttributeValue> sourcedModifiers)
        {
            return cache.TryGetValue(attribute, out sourcedModifiers);
        }
        
        #endregion
        
        public void Multiply(IAttribute attribute, AttributeValue operand, bool allowSelfModify, List<ITag> contextTags, bool anyContextTag)
        {
            if (!ValidateAttribute(attribute)) return;
            
            for (int i = 0; i < cache[attribute].Count; i++)
            {
                if (cache[attribute][i].BaseDerivation.GetSource() == cache[attribute][i].BaseDerivation.GetTarget() && !allowSelfModify) continue;
                if (!anyContextTag && !cache[attribute][i].BaseDerivation.GetContextTags().ContainsAll(contextTags)) continue;
                cache[attribute][i] = cache[attribute][i].Multiply(operand);
            }
        }
        
        public void Multiply(IAttribute attribute, ESignPolicy signPolicy, AttributeValue operand, bool allowSelfModify, List<ITag> contextTags, bool anyContextTag)
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

        public void MultiplyAmplify(IAttribute attribute, EImpactType impactType, ESignPolicy signPolicy, AttributeValue operand, bool allowSelfModify, List<ITag> contextTags, bool anyContextTag)
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

        public void MultiplyAttenuate(IAttribute attribute, EImpactType impactType, ESignPolicy signPolicy, AttributeValue operand, bool allowSelfModify, List<ITag> contextTags, bool anyContextTag)
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
        
        public void Add(IAttribute attribute, AttributeValue operand, bool allowSelfModify, List<ITag> contextTags, bool anyContextTag, bool spread = false)
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

        public void Add(IAttribute attribute, ESignPolicy signPolicy, AttributeValue operand, bool allowSelfModify, List<ITag> contextTags, bool anyContextTag, bool spread = false)
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
        
        public void Override(IAttribute attribute, AttributeValue operand, bool allowSelfModify, List<ITag> contextTags, bool anyContextTag)
        {
            if (!ValidateAttribute(attribute)) return;
            
            for (int i = 0; i < cache[attribute].Count; i++)
            {
                if (cache[attribute][i].BaseDerivation.GetSource() == cache[attribute][i].BaseDerivation.GetTarget() && !allowSelfModify) continue;
                if (!anyContextTag && !cache[attribute][i].BaseDerivation.GetContextTags().ContainsAll(contextTags)) continue;
                cache[attribute][i] = cache[attribute][i].Override(operand);
            }
        }
        
        public void Override(IAttribute attribute, ESignPolicy signPolicy, AttributeValue operand, bool allowSelfModify, List<ITag> contextTags, bool anyContextTag)
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
