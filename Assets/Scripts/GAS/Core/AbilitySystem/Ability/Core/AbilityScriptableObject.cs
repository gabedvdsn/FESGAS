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

        [Header("Impact Event Subscriptions")]

        public List<AbilityEventSubscription> AbilityEventSubscription;

        public AbilitySpec Generate(GASComponent Owner, int Level)
        {
            return new AbilitySpec(Owner, this, Level);
        }
    }

    [Serializable]
    public class AbilityEventSubscription
    {
        /// <summary>
        /// Whenever an ability with a subscribed context tag fires the specified event, activate the event workers
        /// </summary>
        public EAbilityEvent Event;
        public List<GameplayTagScriptableObject> SubscribeToContextTags;
        public List<AbstractImpactWorkerScriptableObject> Workers;

        public AbilityEventSubscription(EAbilityEvent _event, List<GameplayTagScriptableObject> subscribeToContextTags, List<AbstractImpactWorkerScriptableObject> workers)
        {
            Event = _event;
            SubscribeToContextTags = subscribeToContextTags;
            Workers = workers;
        }
    }

    public enum EAbilityEvent
    {
        OnActivate,
        OnInitialImpact,
        OnSecondaryImpact,
        OnAnyImpact,
        OnComplete
    }

}
