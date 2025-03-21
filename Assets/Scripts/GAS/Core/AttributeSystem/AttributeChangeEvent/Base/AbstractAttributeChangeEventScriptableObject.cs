using System;
using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public abstract class AbstractAttributeChangeEventScriptableObject : ScriptableObject
    {
        [Header("Attribute Change Event")] 
        
        public List<int> Priorities;
        
        public abstract void PreAttributeChange(GASComponent system, ref Dictionary<AttributeScriptableObject, CachedAttributeValue> attributeCache, SourcedModifiedAttributeCache modifiedAttributeCache);
        public abstract void PostAttributeChange(GASComponent system, ref Dictionary<AttributeScriptableObject, CachedAttributeValue> attributeCache, SourcedModifiedAttributeCache modifiedAttributeCache);

        private void OnValidate()
        {
            if (Priorities is null) Priorities = new List<int>() { 0 };
            else if (Priorities.Count == 0) Priorities.Add(0);
            else
            {
                for (int i = 0; i < Priorities.Count; i++)
                {
                    if (i == 0) continue;
                    for (int j = 0; j < i; j++)
                    {
                        if (Priorities[i] == Priorities[j]) Priorities[i] += 1;
                    }
                }
            }
        }
    }
}
