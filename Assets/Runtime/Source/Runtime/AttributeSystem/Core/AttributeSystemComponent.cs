using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public class AttributeSystemComponent : MonoBehaviour
    {
        protected IAttributeSet attributeSet;
        protected List<AbstractAttributeChangeEventScriptableObject> attributeChangeEvents = new();

        private AttributeChangeMomentHandler PreChangeHandler;
        private AttributeChangeMomentHandler PostChangeHandler;
        
        private Dictionary<IAttribute, CachedAttributeValue> AttributeCache;
        
        private GASComponentBase Root;
        
        #region Initialization
        
        public virtual void Initialize(GASComponentBase system)
        {
            Root = system;

            InitializeCaches();
            InitializePriorityChangeEvents();
            InitializeAttributeSets();
        }

        public void ProvidePrerequisiteData(ISystemData systemData)
        {
            attributeSet = systemData.GetAttributeSet();
            attributeChangeEvents = systemData.GetAttributeChangeEvents();
        }
        
        private void InitializeCaches()
        {
            AttributeCache = new Dictionary<IAttribute, CachedAttributeValue>();
        }

        private void InitializeAttributeSets()
        {
            if (attributeSet is null) return;
            
            attributeSet.Initialize(this);

            foreach (var attr in AttributeCache.Keys)
            {
                ModifyAttribute(attr,
                    new SourcedModifiedAttributeValue(
                        IAttributeImpactDerivation.GenerateSourceDerivation(Root, attr),
                        0f, 0f,
                        false)
                );
            }
        }

        private void InitializePriorityChangeEvents()
        {
            PreChangeHandler = new AttributeChangeMomentHandler();
            PostChangeHandler = new AttributeChangeMomentHandler();
            foreach (AbstractAttributeChangeEventScriptableObject changeEvent in attributeChangeEvents) changeEvent.RegisterWithHandler(PreChangeHandler, PostChangeHandler);
        }
        
        #endregion
        
        #region Management
        
        public bool ProvideChangeEvent(AbstractAttributeChangeEventScriptableObject changeEvent)
        {
            return changeEvent.RegisterWithHandler(PreChangeHandler, PostChangeHandler);
        }

        public bool RescindChangeEvent(AbstractAttributeChangeEventScriptableObject changeEvent)
        {
            return changeEvent.DeRegisterFromHandler(PreChangeHandler, PostChangeHandler);
        }

        public void ProvideAttribute(IAttribute attribute, DefaultAttributeValue defaultValue)
        {
            if (AttributeCache.ContainsKey(attribute)) return;
            
            AttributeCache[attribute] = new CachedAttributeValue(defaultValue.Overflow);
            AttributeCache[attribute].Add(IAttributeImpactDerivation.GenerateSourceDerivation(Root, attribute), defaultValue.ToAttributeValue());
        }
        
        #endregion
        
        #region Helpers
        
        public bool DefinesAttribute(IAttribute attribute) => AttributeCache.ContainsKey(attribute);
        
        public bool TryGetAttributeValue(IAttribute attribute, out CachedAttributeValue attributeValue)
        {
            return AttributeCache.TryGetValue(attribute, out attributeValue);
        }

        public bool TryGetAttributeValue(IAttribute attribute, out AttributeValue attributeValue)
        {
            if (AttributeCache.TryGetValue(attribute, out var cachedValue))
            {
                attributeValue = cachedValue.Value;
                return true;
            }

            attributeValue = default;
            return false;
        }
        
        #endregion
        
        #region Attribute Modification
        
        public void ModifyAttribute(IAttribute attribute, SourcedModifiedAttributeValue sourcedModifiedValue, bool runEvents = true)
        {
            if (!AttributeCache.ContainsKey(attribute)) return;

            // Create a temp value to track during change events
            ChangeValue change = new ChangeValue(sourcedModifiedValue);
            if (runEvents) PreChangeHandler.RunEvents(attribute, Root, AttributeCache, change);
            
            // Hold version of previous attribute value & apply changes
            AttributeValue holdValue = AttributeCache[attribute].Value;
            AttributeCache[attribute].Add(sourcedModifiedValue.BaseDerivation, change.Value.ToModified());
            
            if (runEvents) PostChangeHandler.RunEvents(attribute, Root, AttributeCache, change);
            
            // Override the temp value to reflect real impact (note that all post-change events will receive this version of impact)
            change.Override(AttributeCache[attribute].Value - holdValue);

            // Relay impact to source
            var impactData = AbilityImpactData.Generate(Root, attribute, sourcedModifiedValue, change.Value.ToAttributeValue());
            if (sourcedModifiedValue.BaseDerivation.GetSource().FindAbilitySystem(out var attr)) attr.ProvideFrameImpactDealt(impactData);
        }

        public void RemoveAttributeDerivation(IAttributeImpactDerivation derivation)
        {
            if (!AttributeCache.ContainsKey(derivation.GetAttribute())) return;
            AttributeCache[derivation.GetAttribute()].Remove(derivation);
        }

        public void RemoveAttributeDerivations(List<IAttributeImpactDerivation> derivations)
        {
            foreach (IAttributeImpactDerivation derivation in derivations)
            {
                if (!AttributeCache.ContainsKey(derivation.GetAttribute())) continue;
                AttributeCache[derivation.GetAttribute()].Remove(derivation);
            }
        }

        #endregion
        
    }
}
