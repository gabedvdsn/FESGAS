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
        
        private Dictionary<AttributeScriptableObject, CachedAttributeValue> AttributeCache;
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
            AttributeCache = new Dictionary<AttributeScriptableObject, CachedAttributeValue>();
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
            AttributeCache[attribute] = new CachedAttributeValue();
            
            AttributeCache[attribute].Add(IAttributeDerivation.GenerateSourceDerivation(System), modifiedAttributeValue.ToAttributeValue());
            ModifiedAttributeCache.SubscribeAttribute(attribute);
        }

        private void UpdateAttributes()
        {
            ApplyAttributeModifications();
        }
        
        private void ApplyAttributeModifications()
        {
            if (!modifiedCacheDirty) return;
            
            foreach (AbstractAttributeChangeEventScriptableObject changeEvent in AttributeChangeEvents)
            {
                changeEvent.PreAttributeChange(System, ref AttributeCache, ModifiedAttributeCache);
            }

            foreach (AttributeScriptableObject attribute in ModifiedAttributeCache.Get())
            {
                HoldAttributeCache[attribute] = AttributeCache[attribute].Value;
                if (!ModifiedAttributeCache.TryGetCachedValue(attribute, out List<SourcedModifiedAttributeValue> sourcedModifiers)) continue;
                foreach (SourcedModifiedAttributeValue sourcedModifier in sourcedModifiers)
                {
                    AttributeCache[attribute].Add(sourcedModifier.Derivation, sourcedModifier.ToModified());
                }
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
                    sourcedModifier.Derivation.GetSource().AbilitySystem.CommunicateAbilityImpact(
                        AbilityImpactData.Generate(
                            attribute, sourcedModifier, AttributeCache[attribute].Value - HoldAttributeCache[attribute]
                        )
                    );
                }
            }

            modifiedCacheDirty = false;
            HoldAttributeCache.Clear();
            ModifiedAttributeCache.Clear();
        }

        public void ModifyAttribute(AttributeScriptableObject attribute, SourcedModifiedAttributeValue sourcedModifiedValue)
        {
            if (!AttributeCache.ContainsKey(attribute)) return;
            
            modifiedCacheDirty = true;
            ModifiedAttributeCache.Add(attribute, sourcedModifiedValue);
        }

        public void ModifyAttributeImmediate(AttributeScriptableObject attribute, SourcedModifiedAttributeValue sourcedModifiedValue)
        {
            
        }

        public bool TryGetAttributeValue(AttributeScriptableObject attribute, out CachedAttributeValue attributeValue)
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
