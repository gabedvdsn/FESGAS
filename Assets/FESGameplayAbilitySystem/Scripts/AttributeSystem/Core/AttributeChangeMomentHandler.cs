using System.Collections.Generic;
using System.Linq;

namespace FESGameplayAbilitySystem
{
    public class AttributeChangeMomentHandler
    {
        public Dictionary<AttributeScriptableObject, List<AbstractAttributeChangeEventScriptableObject>> ChangeEvents = new();

        public bool AddEvent(AttributeScriptableObject attribute, AbstractAttributeChangeEventScriptableObject changeEvent)
        {
            if (ChangeEvents.ContainsKey(attribute))
            {
                if (ChangeEvents[attribute].Contains(changeEvent)) return false;
                ChangeEvents[attribute].Add(changeEvent);
            }
            else ChangeEvents[attribute] = new List<AbstractAttributeChangeEventScriptableObject>() { changeEvent };
                
            return true;
        }
            
        public bool RemoveEvent(AttributeScriptableObject attribute, AbstractAttributeChangeEventScriptableObject changeEvent)
        {
            if (!ChangeEvents.ContainsKey(attribute)) return false;
                
            ChangeEvents[attribute].Remove(changeEvent);
            if (ChangeEvents[attribute].Count == 0)
            {
                ChangeEvents.Remove(attribute);
            }
                
            return true;
        }
            
        public void RunEvents(AttributeScriptableObject attribute, GASComponentBase system, Dictionary<AttributeScriptableObject, CachedAttributeValue> attributeCache,
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
