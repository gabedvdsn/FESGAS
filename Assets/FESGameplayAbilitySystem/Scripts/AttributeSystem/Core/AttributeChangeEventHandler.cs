using System.Collections.Generic;
using System.Linq;

namespace FESGameplayAbilitySystem
{
    public class AttributeChangeEventHandler
    {
        private AttributeChangeMomentHandler PreChangeEvents = new();
        private AttributeChangeMomentHandler PostChangeEvents = new();

        public void RunPreChangeEvents(GASComponentBase system, ref Dictionary<AttributeScriptableObject, CachedAttributeValue> attributeCache,
            SourcedModifiedAttributeCache modifiedAttributeCache)
        {
            foreach (AttributeScriptableObject attribute in modifiedAttributeCache.GetModified()) PreChangeEvents.RunEvents(attribute, system, ref attributeCache, modifiedAttributeCache);
        }

        public void RunPostChangeEvents(GASComponentBase system, ref Dictionary<AttributeScriptableObject, CachedAttributeValue> attributeCache,
            SourcedModifiedAttributeCache modifiedAttributeCache)
        {
            foreach (AttributeScriptableObject attribute in modifiedAttributeCache.GetModified()) PostChangeEvents.RunEvents(attribute, system, ref attributeCache, modifiedAttributeCache);
        }

        public bool AddPreChangeEvent(AttributeScriptableObject attribute, AbstractAttributeChangeEventScriptableObject changeEvent)
        {
            return PreChangeEvents.AddEvent(attribute, changeEvent);
        }
        
        public bool AddPostChangeEvent(AttributeScriptableObject attribute, AbstractAttributeChangeEventScriptableObject changeEvent)
        {
            return PostChangeEvents.AddEvent(attribute, changeEvent);
        }
        
        public bool RemovePreChangeEvent(AttributeScriptableObject attribute, AbstractAttributeChangeEventScriptableObject changeEvent)
        {
            return PreChangeEvents.RemoveEvent(attribute, changeEvent);
        }
        
        public bool RemovePostChangeEvent(AttributeScriptableObject attribute, AbstractAttributeChangeEventScriptableObject changeEvent)
        {
            return PostChangeEvents.RemoveEvent(attribute, changeEvent);
        }

        private class AttributeChangeMomentHandler
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
            
            public void RunEvents(AttributeScriptableObject attribute, GASComponentBase system, ref Dictionary<AttributeScriptableObject, CachedAttributeValue> attributeCache,
                SourcedModifiedAttributeCache modifiedAttributeCache)
            {
                if (!ChangeEvents.ContainsKey(attribute)) return;
                foreach (var fEvent in ChangeEvents[attribute]) fEvent.AttributeChangeEvent(system, ref attributeCache, modifiedAttributeCache);
            }
        }
    }
}
