using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(menuName = "FESGAS/Attribute/Change Event/Group", fileName = "ACE_Group")]
    public class AttributeChangeEventGroupScriptableObject : AbstractAttributeChangeEventScriptableObject
    {
        public List<AbstractAttributeChangeEventScriptableObject> ChangeEvents;
        
        public override void PreAttributeChange(GASComponentBase system, ref Dictionary<AttributeScriptableObject, CachedAttributeValue> attributeCache,
            SourcedModifiedAttributeCache modifiedAttributeCache)
        {
            foreach (AbstractAttributeChangeEventScriptableObject fEvent in ChangeEvents)
            {
                if (!fEvent.ValidateWorkFor(system, ref attributeCache, modifiedAttributeCache)) continue;
                fEvent.PreAttributeChange(system, ref attributeCache, modifiedAttributeCache);
            }
        }
        public override void PostAttributeChange(GASComponentBase system, ref Dictionary<AttributeScriptableObject, CachedAttributeValue> attributeCache,
            SourcedModifiedAttributeCache modifiedAttributeCache)
        {
            foreach (AbstractAttributeChangeEventScriptableObject fEvent in ChangeEvents) fEvent.PostAttributeChange(system, ref attributeCache, modifiedAttributeCache);
        }
        public override List<AttributeScriptableObject> GetKeyAttributes()
        {
            List<AttributeScriptableObject> keys = new List<AttributeScriptableObject>();
            foreach (AbstractAttributeChangeEventScriptableObject changeEvent in ChangeEvents)
            {
                foreach (AttributeScriptableObject attribute in changeEvent.GetKeyAttributes().Where(attribute => !keys.Contains(attribute)))
                {
                    keys.Add(attribute);
                }
            }

            return keys;
        }

        public override List<AbstractAttributeChangeEventScriptableObject> GetValueChangeEvents()
        {
            List<AbstractAttributeChangeEventScriptableObject> changeEvents = new List<AbstractAttributeChangeEventScriptableObject>();
            foreach (AbstractAttributeChangeEventScriptableObject groupEvent in ChangeEvents)
            {
                foreach (AbstractAttributeChangeEventScriptableObject changeEvent in groupEvent.GetValueChangeEvents().Where(changeEvent => !changeEvents.Contains(changeEvent)))
                {
                    changeEvents.Add(changeEvent);
                }
            }

            return changeEvents;
        }
        public override bool ValidateWorkFor(GASComponentBase system, ref Dictionary<AttributeScriptableObject, CachedAttributeValue> attributeCache,
            SourcedModifiedAttributeCache modifiedAttributeCache)
        {
            foreach (var changeEvent in ChangeEvents)
            {
                if (changeEvent.ValidateWorkFor(system, ref attributeCache, modifiedAttributeCache)) return true;
            }

            return false;
        }
    }
}
