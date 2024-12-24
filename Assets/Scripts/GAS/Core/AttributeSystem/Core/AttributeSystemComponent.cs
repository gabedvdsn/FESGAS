using System;
using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public class AttributeSystemComponent : MonoBehaviour
    {
        public AttributeSetScriptableObject AttributeSet;
        
        
        private Dictionary<AttributeScriptableObject, AttributeValue> AttributeCache;
        private Dictionary<AttributeScriptableObject, ModifiedAttributeValue> ModifiedAttributeCache;
        private bool modifiedCacheDirty;

        // Order of events should be considered (e.g. clamping before damage numbers)
        // Pre: changing modified attribute values to reflect weakness, amplification, etc...
        // Post: clamping max values, damage numbers, etc...
        [SerializeField] private List<AbstractAttributeChangeEventScriptableObject> AttributeChangeEvents;

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
            ModifiedAttributeCache = new Dictionary<AttributeScriptableObject, ModifiedAttributeValue>();
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
            Debug.Log($"Provided {attribute.Name} for {modifiedAttributeValue.ToString()}");
            AttributeCache[attribute] = modifiedAttributeValue.ToAttributeValue();
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
                changeEvent.PreAttributeChange(System, ref AttributeCache, ref ModifiedAttributeCache);
            }
            
            foreach (AttributeScriptableObject attribute in ModifiedAttributeCache.Keys)
            {
                AttributeValue oldAttributeValue = AttributeCache[attribute];
                AttributeCache[attribute] = oldAttributeValue.ApplyModified(ModifiedAttributeCache[attribute]);
            }
            
            foreach (AbstractAttributeChangeEventScriptableObject changeEvent in AttributeChangeEvents)
            {
                changeEvent.PostAttributeChange(System, ref AttributeCache, ref ModifiedAttributeCache);
            }

            modifiedCacheDirty = false;
            ModifiedAttributeCache.Clear();
        }

        private void OverrideAttributeValue(AttributeScriptableObject attribute, AttributeValue overrideAttributeValue)
        {
            // Override is not added to modification cache
            AttributeCache[attribute] = overrideAttributeValue;
        }

        public void ModifyAttribute(AttributeScriptableObject attribute, ModifiedAttributeValue modifiedAttributeValue)
        {
            if (!AttributeCache.ContainsKey(attribute)) return;
            
            modifiedCacheDirty = true;
            if (!TryGetModifiedAttributeValue(attribute, out ModifiedAttributeValue currModifiedAttributeValue))
            {
                ModifiedAttributeCache[attribute] = modifiedAttributeValue;
            }
            else ModifiedAttributeCache[attribute] = currModifiedAttributeValue.Combine(modifiedAttributeValue);
        }

        public bool TryGetAttributeValue(AttributeScriptableObject attribute, out AttributeValue attributeValue)
        {
            if (attribute) return AttributeCache.TryGetValue(attribute, out attributeValue);
            
            attributeValue = default;
            return false;
        }

        public bool TryGetModifiedAttributeValue(AttributeScriptableObject attribute, out ModifiedAttributeValue modifiedAttributeValue)
        {
            if (attribute) return ModifiedAttributeCache.TryGetValue(attribute, out modifiedAttributeValue);

            modifiedAttributeValue = default;
            return false;
        }
    }
}
