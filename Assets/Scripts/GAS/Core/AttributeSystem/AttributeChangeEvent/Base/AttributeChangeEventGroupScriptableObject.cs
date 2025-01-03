﻿using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(menuName = "FESGAS/Authored/Attribute Change Event/Grouop", fileName = "ACE_Group")]
    public class AttributeChangeEventGroupScriptableObject : AbstractAttributeChangeEventScriptableObject
    {
        public List<AbstractAttributeChangeEventScriptableObject> FocusedEvents;
        
        public override void PreAttributeChange(GASComponent system, ref Dictionary<AttributeScriptableObject, AttributeValue> attributeCache, ref Dictionary<AttributeScriptableObject, ModifiedAttributeValue> modifiedAttributeCache)
        {
            foreach (AbstractAttributeChangeEventScriptableObject fEvent in FocusedEvents) fEvent.PreAttributeChange(system, ref attributeCache, ref modifiedAttributeCache);
        }
        public override void PostAttributeChange(GASComponent system, ref Dictionary<AttributeScriptableObject, AttributeValue> attributeCache, ref Dictionary<AttributeScriptableObject, ModifiedAttributeValue> modifiedAttributeCache)
        {
            foreach (AbstractAttributeChangeEventScriptableObject fEvent in FocusedEvents) fEvent.PostAttributeChange(system, ref attributeCache, ref modifiedAttributeCache);
        }
    }
}