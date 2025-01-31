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
        //private Dictionary<AttributeScriptableObject, ModifiedAttributeValue> ModifiedAttributeCache;
        public SourcedModifiedAttributeCache SourcedMAC;
        private bool modifiedCacheDirty;

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
            //ModifiedAttributeCache = new Dictionary<AttributeScriptableObject, ModifiedAttributeValue>();
            SourcedMAC = new SourcedModifiedAttributeCache();
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
            SourcedMAC.SubscribeAttribute(attribute);
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
                changeEvent.PreAttributeChange(System, ref AttributeCache, SourcedMAC);
            }
            
            foreach (AttributeScriptableObject attribute in SourcedMAC.Get())
            {
                Debug.Log(SourcedMAC.ToModified(attribute));
                AttributeValue newAttributeValue = AttributeCache[attribute].ApplyModified(SourcedMAC.ToModified(attribute));
                AttributeCache[attribute] = newAttributeValue;
            }
            
            foreach (AbstractAttributeChangeEventScriptableObject changeEvent in AttributeChangeEvents)
            {
                //changeEvent.PostAttributeChange(System, ref AttributeCache, ref ModifiedAttributeCache);
                changeEvent.PostAttributeChange(System, ref AttributeCache, SourcedMAC);
            }

            modifiedCacheDirty = false;
            //ModifiedAttributeCache.Clear();
            SourcedMAC.Clear();
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
            /*if (!TryGetModifiedAttributeValue(attribute, out ModifiedAttributeValue currModifiedAttributeValue))
            {
                ModifiedAttributeCache[attribute] = sourcedModifiedValue.ToModified();
            }
            else ModifiedAttributeCache[attribute] = currModifiedAttributeValue.Combine(sourcedModifiedValue.ToModified());
            */

            SourcedMAC.Add(attribute, sourcedModifiedValue);
        }

        private void OnAttributeModified(AttributeScriptableObject attribute, SourcedModifiedAttributeValue sourcedModifiedValue)
        {
            
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
            if (attribute) return SourcedMAC.TryToModified(attribute, out modifiedAttributeValue);
            
            modifiedAttributeValue = default;
            return false;
        }
    }
}
