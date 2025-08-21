using System;
using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public abstract class AbstractAttributeChangeEventScriptableObject : ScriptableObject
    {
        public abstract void AttributeChangeEvent(GASComponentBase system, Dictionary<IAttribute, CachedAttributeValue> attributeCache,
            ChangeValue change);

        public abstract bool ValidateWorkFor(GASComponentBase system, Dictionary<IAttribute, CachedAttributeValue> attributeCache,
            ChangeValue change);

        public abstract bool RegisterWithHandler(AttributeChangeMomentHandler preChange, AttributeChangeMomentHandler postChange);

        public abstract bool DeRegisterFromHandler(AttributeChangeMomentHandler preChange, AttributeChangeMomentHandler postChange);

        public virtual int GetPriority() => 0;

        protected virtual void InternalValidate()
        {
            
        }

        private void OnValidate()
        {
            InternalValidate();
        }

        public override string ToString()
        {
            return name;
        }

        public enum EChangeEventTiming
        {
            PreChange,
            PostChange,
            Both
        }
    }
}
