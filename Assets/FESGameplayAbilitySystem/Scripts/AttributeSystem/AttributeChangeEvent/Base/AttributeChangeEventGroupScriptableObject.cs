using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using UnityEngine;
using UnityEngine.Serialization;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(menuName = "FESGAS/Attribute/Change Event/Group", fileName = "ACE_Group")]
    public class AttributeChangeEventGroupScriptableObject : AbstractAttributeChangeEventScriptableObject
    {
        public List<AbstractAttributeChangeEventScriptableObject> ChangeEvents;
        
        public override void AttributeChangeEvent(GASComponentBase system, ref Dictionary<AttributeScriptableObject, CachedAttributeValue> attributeCache,
            SourcedModifiedAttributeCache modifiedAttributeCache)
        {
            foreach (AbstractAttributeChangeEventScriptableObject fEvent in ChangeEvents)
            {
                if (!fEvent.ValidateWorkFor(system, ref attributeCache, modifiedAttributeCache)) continue;
                fEvent.AttributeChangeEvent(system, ref attributeCache, modifiedAttributeCache);
            }
        }

        public override bool ValidateWorkFor(GASComponentBase system, ref Dictionary<AttributeScriptableObject, CachedAttributeValue> attributeCache,
            SourcedModifiedAttributeCache modifiedAttributeCache)
        {
            foreach (var changeEvent in ChangeEvents)
            {
                // If any of the events are valid return true
                if (changeEvent.ValidateWorkFor(system, ref attributeCache, modifiedAttributeCache)) return true;
            }

            return false;
        }

        public override bool RegisterWithHandler(AttributeChangeEventHandler handler)
        {
            return ChangeEvents.Any(changeEvent => changeEvent.RegisterWithHandler(handler));
        }
        public override bool DeRegisterFromHandler(AttributeChangeEventHandler handler)
        {
            return ChangeEvents.Any(changeEvent => changeEvent.DeRegisterFromHandler(handler));
        }
    }
}
