using System;
using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public abstract class AbstractAttributeChangeEvent
    {
        public abstract void AttributeChangeEvent(GASComponent system, Dictionary<Attribute, CachedAttributeValue> attributeCache,
            ChangeValue change);

        public abstract bool ValidateWorkFor(GASComponent system, Dictionary<Attribute, CachedAttributeValue> attributeCache,
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

        public enum EChangeEventTiming
        {
            PreChange,
            PostChange,
            Both
        }
    }
}
