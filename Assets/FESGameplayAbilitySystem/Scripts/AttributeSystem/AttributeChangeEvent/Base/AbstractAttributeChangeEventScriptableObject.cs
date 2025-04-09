using System;
using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public abstract class AbstractAttributeChangeEventScriptableObject : ScriptableObject
    {
        public abstract void AttributeChangeEvent(GASComponentBase system, ref Dictionary<AttributeScriptableObject, CachedAttributeValue> attributeCache,
            SourcedModifiedAttributeCache modifiedAttributeCache);

        public abstract bool ValidateWorkFor(GASComponentBase system, ref Dictionary<AttributeScriptableObject, CachedAttributeValue> attributeCache,
            SourcedModifiedAttributeCache modifiedAttributeCache);

        public abstract bool RegisterWithHandler(AttributeChangeEventHandler handler);

        public abstract bool DeRegisterFromHandler(AttributeChangeEventHandler handler);

        public virtual int GetPriority() => 0;

        protected virtual void InternalValidate()
        {
            
        }

        private void OnValidate()
        {
            InternalValidate();
        }

        public enum EChangeEventTiming
        {
            PreChange,
            PostChange,
            Both
        }
    }
}
