using System;
using System.Collections.Generic;
using FESGameplayAbilitySystem.Core.AttributeSystem.AttributeWorker.Core;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public class AttributeSystemComponent : MonoBehaviour
    {
        [Header("Attributes")]
        
        public AttributeSetScriptableObject AttributeSet;
        
        [Header("Attribute Change Events")]
        
        public List<AbstractAttributeChangeEventScriptableObject> AttributeChangeEvents;

        [Header("Attribute Workers")] 
        
        public List<AbstractAttributeWorkerScriptableObject> AttributeWorkers;
        
        private Dictionary<AttributeScriptableObject, AttributeValue> AttributeCache;
        private SourcedModifiedAttributeCache ModifiedAttributeCache;
        private bool modifiedCacheDirty;

        private Dictionary<AttributeScriptableObject, AttributeValue> HoldAttributeCache;

        // Order of events should be considered (e.g. clamping before damage numbers)
        // Pre: changing modified attribute values to reflect weakness, amplification, etc...
        // Post: clamping max values, damage numbers, etc...

        private GASComponent System;

        private void Awake()
        {
            System = GetComponent<GASComponent>();
            
            InitializeCaches();
            InitializeAttributeSets();
        }
        
        private void LateUpdate()
        {
            UpdateAttributes();
        }

        private void InitializeCaches()
        {
            AttributeCache = new Dictionary<AttributeScriptableObject, AttributeValue>();
            ModifiedAttributeCache = new SourcedModifiedAttributeCache();
            HoldAttributeCache = new Dictionary<AttributeScriptableObject, AttributeValue>();
        }

        private void InitializeAttributeSets()
        {
            AttributeSet.Initialize(this);
            
            modifiedCacheDirty = true;
            UpdateAttributes();
        }

        public void ProvideAttribute(AttributeScriptableObject attribute, ModifiedAttributeValue modifiedAttributeValue)
        {
            if (AttributeCache.ContainsKey(attribute)) return;
            
            AttributeCache[attribute] = modifiedAttributeValue.ToAttributeValue();
            ModifiedAttributeCache.SubscribeAttribute(attribute);
        }

        private void UpdateAttributes()
        {
            ApplyAttributeModifications();
        }

        private void ApplyAttributeWorkerModifications()
        {
            foreach (AbstractAttributeWorkerScriptableObject worker in AttributeWorkers)
            {
                
            }
        }
        
        private void ApplyAttributeModifications()
        {
            if (!modifiedCacheDirty) return;
            
            foreach (AbstractAttributeChangeEventScriptableObject changeEvent in AttributeChangeEvents)
            {
                //changeEvent.PreAttributeChange(System, ref AttributeCache, ref ModifiedAttributeCache);
                changeEvent.PreAttributeChange(System, ref AttributeCache, ModifiedAttributeCache);
            }

            foreach (AttributeScriptableObject attribute in ModifiedAttributeCache.Get())
            {
                HoldAttributeCache[attribute] = AttributeCache[attribute];
                AttributeValue newAttributeValue = AttributeCache[attribute].ApplyModified(ModifiedAttributeCache.ToModified(attribute));
                AttributeCache[attribute] = newAttributeValue;
            }
            
            foreach (AbstractAttributeChangeEventScriptableObject changeEvent in AttributeChangeEvents)
            {
                changeEvent.PostAttributeChange(System, ref AttributeCache, ModifiedAttributeCache);
            }

            // Communicate the impact of the modification back to the source
            foreach (AttributeScriptableObject attribute in HoldAttributeCache.Keys)
            {
                if (!ModifiedAttributeCache.TryGetCachedValue(attribute, out var sourcedModifiers)) continue;
                foreach (SourcedModifiedAttributeValue sourcedModifier in sourcedModifiers)
                {
                    sourcedModifier.SourceSpec.Source.AbilitySystem.CommunicateAbilityImpact(
                        AbilityImpactData.Generate(
                            attribute, sourcedModifier, AttributeCache[attribute] - HoldAttributeCache[attribute]
                        )
                    );
                }
            }

            modifiedCacheDirty = false;
            HoldAttributeCache.Clear();
            ModifiedAttributeCache.Clear();
        }
        
        private void OverrideAttributeValue(AttributeScriptableObject attribute, AttributeValue overrideAttributeValue)
        {
            // Override is not added to modification cache
            AttributeCache[attribute] = overrideAttributeValue;
        }

        public void ModifyAttribute(AttributeScriptableObject attribute, SourcedModifiedAttributeValue sourcedModifiedValue)
        {
            if (!AttributeCache.ContainsKey(attribute)) return;
            
            modifiedCacheDirty = true;
            ModifiedAttributeCache.Add(attribute, sourcedModifiedValue);
        }

        public bool TryGetAttributeValue(AttributeScriptableObject attribute, out AttributeValue attributeValue)
        {
            if (attribute) return AttributeCache.TryGetValue(attribute, out attributeValue);
            
            attributeValue = default;
            return false;
        }

        public bool TryGetModifiedAttributeValue(AttributeScriptableObject attribute, out ModifiedAttributeValue modifiedAttributeValue)
        {
            // if (attribute) return ModifiedAttributeCache.TryGetCachedValue(attribute, out modifiedAttributeValue);
            if (attribute) return ModifiedAttributeCache.TryToModified(attribute, out modifiedAttributeValue);
            
            modifiedAttributeValue = default;
            return false;
        }
    }
}
