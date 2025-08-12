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
    public class AbilityScriptableObject : ScriptableObject, IAbilityData
    {
        [Header("Ability")]
        
        public AbilityDefinition Definition;
        public AbilityTags Tags;
        public AbilityProxySpecification Proxy;
        
        [Header("Leveling")]

        public int StartingLevel = 1;
        public int MaxLevel = 4;
        public bool IgnoreWhenLevelZero = true;

        [Header("Using")]
        
        public GameplayEffectScriptableObject Cost;
        public GameplayEffectScriptableObject Cooldown;

        public AbilityDefinition GetDefinition()
        {
            return Definition;
        }
        public AbilityTags GetTags()
        {
            return Tags;
        }
        public AbilityProxySpecification GetProxy()
        {
            return Proxy;
        }
        public int GetStartingLevel()
        {
            return StartingLevel;
        }
        public int GetMaxLevel()
        {
            return MaxLevel;
        }
        public bool GetIgnoreWhenLevelZero()
        {
            return IgnoreWhenLevelZero;
        }
        public GameplayEffectScriptableObject GetCost()
        {
            return Cost;
        }
        public GameplayEffectScriptableObject GetCooldown()
        {
            return Cooldown;
        }

        public override string ToString()
        {
            return Tags.AssetTag.Name;
        }
    }

    public interface IAbilityData
    {
        public AbilityDefinition GetDefinition();
        public AbilityTags GetTags();
        public AbilityProxySpecification GetProxy();
        
        public int GetStartingLevel();
        public int GetMaxLevel();
        public bool GetIgnoreWhenLevelZero();
        
        public GameplayEffectScriptableObject GetCost();
        public GameplayEffectScriptableObject GetCooldown();

        public AbilitySpec Generate(GASComponentBase owner, int level)
        {
            return AbilitySpec.Generate(this, owner, level);
        }
    }
}
