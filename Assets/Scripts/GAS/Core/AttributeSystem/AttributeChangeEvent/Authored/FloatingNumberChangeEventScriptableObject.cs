using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(menuName = "FESGAS/Authored/Attribute Change Event/Floating Number", fileName = "ACE_FloatingNumber")]
    public class FloatingNumberChangeEventScriptableObject : AbstractFocusedAttributeChangeEventScriptableObject
    {

        public override void PreAttributeChange(GASComponent system, ref Dictionary<AttributeScriptableObject, AttributeValue> attributeCache, SourcedModifiedAttributeCache modifiedAttributeCache)
        {
            // Floating numbers event shouldn't implement anything here
        }
        
        public override void PostAttributeChange(GASComponent system, ref Dictionary<AttributeScriptableObject, AttributeValue> attributeCache, SourcedModifiedAttributeCache modifiedAttributeCache)
        {
            if (!modifiedAttributeCache.TryToModified(PrimaryAttribute, out ModifiedAttributeValue modifiedAttributeValue)) return;
            
            // Instantiate and supply modification value
        }
    }
}
