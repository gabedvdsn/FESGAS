using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(menuName = "FESGAS/Ability/Ability", fileName = "ABIL_")]
    public class AbilityScriptableObject : ScriptableObject
    {
        [Header("Ability")]
        
        public AbilityDefinition Definition;
        public AbilityTags Tags;
        public AbilityProxySpecification Proxy;
        
        [Header("Leveling")]

        public int StartingLevel = 0;
        public int MaxLevel = 4;
        public bool IgnoreWhenLevelZero = true;

        [Header("Using")]
        
        public GameplayEffectScriptableObject Cost;
        public GameplayEffectScriptableObject Cooldown;
        
        [Header("Impact Workers")]
        
        public List<AbstractAbilityImpactWorkerScriptableObject> ImpactWorkers;

        [Header("Impact Subscriptions")]

        public List<AbilityScriptableObject> ImpactSubscriptions;

        public AbilitySpec Generate(GASComponent Owner, int Level)
        {
            return new AbilitySpec(Owner, this, Level);
        }
    }

}
