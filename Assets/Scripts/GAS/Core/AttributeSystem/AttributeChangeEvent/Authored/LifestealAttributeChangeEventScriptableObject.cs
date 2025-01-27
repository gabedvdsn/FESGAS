using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(menuName = "FESGAS/Authored/Attribute Change Event/Lifesteal", fileName = "ACE_Lifesteal")]
    public class LifestealAttributeChangeEventScriptableObject : AbstractFocusedAttributeChangeEventScriptableObject
    {
        public override void PreAttributeChange(GASComponent system, ref Dictionary<AttributeScriptableObject, AttributeValue> attributeCache, SourcedModifiedAttributeCache modifiedAttributeCache)
        {
            if (!attributeCache.ContainsKey(PrimaryAttribute)) return;
            
            
        }
        
        public override void PostAttributeChange(GASComponent system, ref Dictionary<AttributeScriptableObject, AttributeValue> attributeCache, SourcedModifiedAttributeCache modifiedAttributeCache)
        {
            throw new System.NotImplementedException();
        }
    }
}
