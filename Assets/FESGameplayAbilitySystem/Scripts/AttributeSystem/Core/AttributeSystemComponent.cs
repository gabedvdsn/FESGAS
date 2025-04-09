using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public class AttributeSystemComponent : MonoBehaviour
    {
        protected AttributeSetScriptableObject attributeSet;
        protected List<AbstractAttributeChangeEventScriptableObject> attributeChangeEvents;
        
        private AttributeChangeEventHandler ChangeEventHandler;
        
        private Dictionary<AttributeScriptableObject, CachedAttributeValue> AttributeCache;
        private SourcedModifiedAttributeCache ModifiedAttributeCache;
        private bool modifiedCacheDirty;

        private Dictionary<AttributeScriptableObject, AttributeValue> HoldAttributeCache;

        // Order of events should be considered (e.g. clamping before damage numbers)
        // Pre: changing modified attribute values to reflect weakness, amplification, etc...
        // Post: clamping max values, damage numbers, etc...

        private GASComponentBase System;

        public virtual void Initialize(GASComponentBase system)
        {
            System = system;
            ChangeEventHandler = new AttributeChangeEventHandler();
            
            InitializeCaches();
            InitializePriorityChangeEvents();
            InitializeAttributeSets();
        }

        public void ProvidePrerequisiteData(GASSystemData systemData)
        {
            attributeSet = systemData.AttributeSet;
            attributeChangeEvents = systemData.AttributeChangeEvents;
        }
        
        private void LateUpdate()
        {
            UpdateAttributes();
            System.AttributeSystemFinished();
        }

        private void InitializeCaches()
        {
            AttributeCache = new Dictionary<AttributeScriptableObject, CachedAttributeValue>();
            ModifiedAttributeCache = new SourcedModifiedAttributeCache();
            HoldAttributeCache = new Dictionary<AttributeScriptableObject, AttributeValue>();
        }

        private void InitializeAttributeSets()
        {
            attributeSet.Initialize(this);
            
            foreach (AttributeScriptableObject attribute in ModifiedAttributeCache.GetDefined())
            {
                ModifyAttribute(attribute, new SourcedModifiedAttributeValue(IAttributeImpactDerivation.GenerateSourceDerivation(System, attribute), 0f, 0f, false));
            }
            
            UpdateAttributes();
        }

        private void InitializePriorityChangeEvents()
        {
            ChangeEventHandler = new AttributeChangeEventHandler();
            foreach (AbstractAttributeChangeEventScriptableObject changeEvent in attributeChangeEvents) changeEvent.RegisterWithHandler(ChangeEventHandler);
        }

        public bool ProvideChangeEvent(AbstractAttributeChangeEventScriptableObject changeEvent)
        {
            return changeEvent.RegisterWithHandler(ChangeEventHandler);
        }

        public bool RescindChangeEvent(AbstractAttributeChangeEventScriptableObject changeEvent)
        {
            return changeEvent.DeRegisterFromHandler(ChangeEventHandler);
        }

        public void ProvideAttribute(AttributeScriptableObject attribute, DefaultAttributeValue defaultValue)
        {
            if (AttributeCache.ContainsKey(attribute)) return;
            AttributeCache[attribute] = new CachedAttributeValue(defaultValue.Overflow);
            
            AttributeCache[attribute].Add(IAttributeImpactDerivation.GenerateSourceDerivation(System, attribute), defaultValue.ToAttributeValue());
            ModifiedAttributeCache.SubscribeModifiableAttribute(attribute);
        }

        public bool DefinesAttribute(AttributeScriptableObject attribute) => AttributeCache.ContainsKey(attribute);

        private void UpdateAttributes(bool allowWorkers = true)
        {
            ApplyAttributeModifications(allowWorkers);
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
        
        private void ApplyAttributeModifications(bool allowWorkers = true)
        {
            if (!modifiedCacheDirty) return;
            
            ChangeEventHandler.RunPreChangeEvents(System, ref AttributeCache, ModifiedAttributeCache);

            foreach (AttributeScriptableObject attribute in ModifiedAttributeCache.GetModified())
            {
                HoldAttributeCache[attribute] = AttributeCache[attribute].Value;
                if (!ModifiedAttributeCache.TryGetSourcedModifiers(attribute, out List<SourcedModifiedAttributeValue> sourcedModifiers)) continue;
                foreach (SourcedModifiedAttributeValue sourcedModifier in sourcedModifiers)
                {
                    AttributeCache[attribute].Add(sourcedModifier.BaseDerivation, sourcedModifier.ToModified());
                }
            }
            
            ChangeEventHandler.RunPostChangeEvents(System, ref AttributeCache, ModifiedAttributeCache);
            
            HashSet<GASComponentBase> communicateComps = new HashSet<GASComponentBase>();
            
            // Communicate the impact of the modification back to the source
            foreach (AttributeScriptableObject attribute in HoldAttributeCache.Keys)
            {
                if (!ModifiedAttributeCache.TryGetSourcedModifiers(attribute, out var sourcedModifiers)) continue;
                foreach (SourcedModifiedAttributeValue sourcedModifier in sourcedModifiers)
                {
                    sourcedModifier.BaseDerivation.GetSource().AbilitySystem.CommunicateAbilityImpact(
                        AbilityImpactData.Generate(
                            System, attribute, sourcedModifier, AttributeCache[attribute].Value - HoldAttributeCache[attribute]
                        )
                    );
                    communicateComps.Add(sourcedModifier.BaseDerivation.GetSource());
                }
                
                AttributeCache[attribute].Clean();
            }
            
            modifiedCacheDirty = false;
            HoldAttributeCache.Clear();
            ModifiedAttributeCache.Clear();

            if (!allowWorkers) return;
            foreach (GASComponentBase comp in communicateComps) comp.AbilitySystem.ActivateAbilityImpactWorkers();
        }

        public void ModifyAttribute(AttributeScriptableObject attribute, SourcedModifiedAttributeValue sourcedModifiedValue)
        {
            if (!AttributeCache.ContainsKey(attribute)) return;
            
            modifiedCacheDirty = true;
            ModifiedAttributeCache.Register(attribute, sourcedModifiedValue);
        }

        public void ModifyAttributeImmediate(AttributeScriptableObject attribute, SourcedModifiedAttributeValue sourcedModifiedValue, bool allowWorkers = true)
        {
            ModifyAttribute(attribute, sourcedModifiedValue);
            UpdateAttributes(allowWorkers);
        }

        public bool TryGetAttributeValue(AttributeScriptableObject attribute, out CachedAttributeValue attributeValue)
        {
            return AttributeCache.TryGetValue(attribute, out attributeValue);
        }

        public bool TryGetAttributeValue(AttributeScriptableObject attribute, out AttributeValue attributeValue)
        {
            if (AttributeCache.TryGetValue(attribute, out var cachedValue))
            {
                attributeValue = cachedValue.Value;
                return true;
            }

            attributeValue = default;
            return false;
        }

        
    }
}
