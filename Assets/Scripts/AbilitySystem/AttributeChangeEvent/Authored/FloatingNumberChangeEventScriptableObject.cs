using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(menuName = "FESGAS/Authored/Attribute Change Event/Floating Number", fileName = "New Floating Number Event")]
    public class FloatingNumberChangeEventScriptableObject : AbstractAttributeChangeEventScriptableObject
    {

        public override void PreAttributeChange(AttributeSystemComponent abilitySystem, ref Dictionary<AttributeScriptableObject, AttributeValue> attributeCache, ref Dictionary<AttributeScriptableObject, ModifiedAttributeValue> modifiedAttributeCache)
        {
            // Floating numbers event shouldn't implement anything here
        }
        
        public override void PostAttributeChange(AttributeSystemComponent abilitySystem, ref Dictionary<AttributeScriptableObject, AttributeValue> attributeCache, ref Dictionary<AttributeScriptableObject, ModifiedAttributeValue> modifiedAttributeCache)
        {
            if (!modifiedAttributeCache.TryGetValue(PrimaryAttribute, out ModifiedAttributeValue modifiedAttributeValue)) return;
            
            // Instantiate and supply modification value
        }
    }
}
