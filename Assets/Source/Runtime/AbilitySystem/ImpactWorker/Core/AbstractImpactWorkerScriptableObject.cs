using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public abstract class AbstractImpactWorkerScriptableObject : ScriptableObject
    {
        [HideInInspector] public string ReadOnlyDescription = "Impact workers are used to interpret and execute with respect to impact data.\nE.g. lifesteal, damage numbers, etc...";
        
        [Header("Impact Worker")]
        
        public bool AcceptUnworkableImpact = false;
        
        public abstract void InterpretImpact(AbilityImpactData impactData);

        public abstract bool ValidateWorkFor(AbilityImpactData impactData);
        public abstract AttributeScriptableObject GetTargetedAttribute();
        public virtual void SubscribeToCache(ImpactWorkerCache cache)
        {
            cache.AddWorker(GetTargetedAttribute(), this);
        }

        public virtual void UnsubscribeFromCache(ImpactWorkerCache cache)
        {
            cache.RemoveWorker(GetTargetedAttribute(), this);
        }
    }
}
