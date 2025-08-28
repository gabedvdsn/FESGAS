using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public class AttributeSystemComponentManual : AttributeSystemComponent
    {
        [Header("Attributes")]
        
        public AttributeSet AttributeSet;
        
        [Header("Attribute Change Events")]
        
        [SerializeField] private List<AbstractAttributeChangeEvent> AttributeChangeEvents;

        public override void Initialize(GASComponent system)
        {
            attributeSet = AttributeSet;
            attributeChangeEvents = AttributeChangeEvents;
            
            base.Initialize(system);
        }
    }
}

