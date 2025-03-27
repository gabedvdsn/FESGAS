using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public class AttributeSystemComponentManual : AttributeSystemComponent
    {
        [Header("Attributes")]
        
        public AttributeSetScriptableObject AttributeSet;
        
        [Header("Attribute Change Events")]
        
        [SerializeField] private List<AbstractAttributeChangeEventScriptableObject> AttributeChangeEvents;

        public override void Initialize(GASComponentBase system)
        {
            attributeSet = AttributeSet;
            attributeChangeEvents = AttributeChangeEvents;
            
            base.Initialize(system);
        }
    }
}

