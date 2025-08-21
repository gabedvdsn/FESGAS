using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public class AttributeChangeMomentHandler
    {
        public Dictionary<IAttribute, List<AbstractAttributeChangeEventScriptableObject>> ChangeEvents = new();

        public bool AddEvent(IAttribute attribute, AbstractAttributeChangeEventScriptableObject changeEvent)
        {
            if (ChangeEvents.ContainsKey(attribute))
            {
                if (ChangeEvents[attribute].Contains(changeEvent)) return false;
                ChangeEvents[attribute].Add(changeEvent);
            }
            else ChangeEvents[attribute] = new List<AbstractAttributeChangeEventScriptableObject>() { changeEvent };
                
            return true;
        }
            
        public bool RemoveEvent(IAttribute attribute, AbstractAttributeChangeEventScriptableObject changeEvent)
        {
            if (!ChangeEvents.ContainsKey(attribute)) return false;
                
            ChangeEvents[attribute].Remove(changeEvent);
            if (ChangeEvents[attribute].Count == 0)
            {
                ChangeEvents.Remove(attribute);
            }
                
            return true;
        }
            
        public void RunEvents(IAttribute attribute, GASComponentBase system, Dictionary<IAttribute, CachedAttributeValue> attributeCache,
            ChangeValue change)
        {
            if (!ChangeEvents.ContainsKey(attribute)) return;
            foreach (var fEvent in ChangeEvents[attribute])
            {
                if (!fEvent.ValidateWorkFor(system, attributeCache, change)) continue;
                fEvent.AttributeChangeEvent(system, attributeCache, change);
            }
        }
    }
}
