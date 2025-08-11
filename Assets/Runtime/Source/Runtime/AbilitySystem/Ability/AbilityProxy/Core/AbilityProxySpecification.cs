using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace FESGameplayAbilitySystem
{
    [Serializable]
    public class AbilityProxySpecification
    {
        [Header("Targeting Instructions")]
        
        public AbstractTargetingProxyTaskScriptableObject TargetingProxy;
        public bool UseImplicitTargeting = true;
        
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
        [Tooltip("After this stage, should the ability usage effects be applied?")]
        public bool ApplyUsageEffects;
    }

    public enum EAnyAllPolicy
    {
        Any,
        All
    }
}
