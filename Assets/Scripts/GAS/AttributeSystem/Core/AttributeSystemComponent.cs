using System;
using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [RequireComponent(typeof(GASComponent))]
    public class AttributeSystemComponent : MonoBehaviour
    {
        public List<AttributeScriptableObject> Attributes;
        public List<float> InitialValues;
        
        private Dictionary<AttributeScriptableObject, AttributeValue> AttributeCache;
        private Dictionary<AttributeScriptableObject, ModifiedAttributeValue> ModifiedAttributeCache;

        // Order of events should be considered (e.g. clamping before damage numbers)
        // Pre: changing modified attribute values to reflect weakness, amplification, etc...
        // Post: clamping max values, damage numbers, etc...
        [SerializeField] private List<AbstractAttributeChangeEventScriptableObject> AttributeChangeEvents;

        private GASComponent System;

        private void Awake()
        {
            System = GetComponent<GASComponent>();
            
            InitializeCaches();
            for (int i = 0; i < Attributes.Count; i++)
            {
                OverrideAttributeValue(Attributes[i], new AttributeValue(InitialValues[i], InitialValues[i]));
            }
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

        private void UpdateAttributes()
        {
            ApplyAttributeModifications();
        }

        private void ApplyAttributeModifications()
        {
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

            ModifiedAttributeCache.Clear();
        }

        private void OverrideAttributeValue(AttributeScriptableObject attribute, AttributeValue overrideAttributeValue)
        {
            // Override is not added to modification cache
            AttributeCache[attribute] = overrideAttributeValue;
        }

        public void ModifyAttribute(AttributeScriptableObject attribute, ModifiedAttributeValue modifiedAttributeValue)
        {
            if (TryGetModifiedAttributeValue(attribute, out ModifiedAttributeValue currModifiedAttributeValue))
            {
                ModifiedAttributeCache[attribute] = currModifiedAttributeValue.Combine(modifiedAttributeValue);
            }
            else ModifiedAttributeCache[attribute] = modifiedAttributeValue;
        }

        public bool TryGetAttributeValue(AttributeScriptableObject attribute, out AttributeValue attributeValue)
        {
            return AttributeCache.TryGetValue(attribute, out attributeValue);
        }

        public bool TryGetModifiedAttributeValue(AttributeScriptableObject attribute, out ModifiedAttributeValue modifiedAttributeValue)
        {
            return ModifiedAttributeCache.TryGetValue(attribute, out modifiedAttributeValue);
        }
    }
}
