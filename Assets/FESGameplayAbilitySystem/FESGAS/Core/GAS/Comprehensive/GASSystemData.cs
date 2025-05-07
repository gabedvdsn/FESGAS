using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(menuName = "FESGAS/GAS System Data", fileName = "GSD_")]
    public class GASSystemData : ScriptableObject
    {
        [Header("Ability Components")] 
        
        public EAbilityActivationPolicy ActivationPolicy;
        public int MaxAbilities;
        public List<AbilityScriptableObject> StartingAbilities;
        
        [Space(5)]
        
        public List<AbstractImpactWorkerScriptableObject> ImpactWorkers;
        
        [Header("Attribute Components")]
        
        public AttributeSetScriptableObject AttributeSet;
        public List<AbstractAttributeChangeEventScriptableObject> AttributeChangeEvents;
        
        [Header("Tag Workers")]
        
        public List<AbstractTagWorkerScriptableObject> TagWorkers;

    }
}
