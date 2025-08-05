using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [Serializable]
    public class AbilityProxySpecification
    {
        [Header("Targeting Instructions")]
        
        public AbstractTargetingProxyTaskScriptableObject TargetingProxy;
        
        [Space]
        
        public bool UseImplicitInstructions = true;
        public ESourceTargetExpanded OwnerAs = ESourceTargetExpanded.Source;
        
        [Header("Proxy Stages")]
        
        public List<AbilityProxyStage> Stages;

        public AbilityProxy GenerateProxy()
        {
            return new AbilityProxy(this);
        }
    }

    [Serializable]
    public class AbilityProxyStage
    {
        public EAnyAllPolicy TaskPolicy;
        public List<AbstractAbilityProxyTaskScriptableObject> Tasks;
    }

    public enum EAnyAllPolicy
    {
        Any,
        All
    }
}
