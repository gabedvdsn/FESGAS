using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace FESGameplayAbilitySystem
{
    public class Ability : IAbilityData
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
        
        public GameplayEffect Cost;
        public GameplayEffect Cooldown;

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
        public GameplayEffect GetCost()
        {
            return Cost;
        }
        public GameplayEffect GetCooldown()
        {
            return Cooldown;
        }

        public override string ToString()
        {
            return Tags.AssetTag.GetName();
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
        
        public GameplayEffect GetCost();
        public GameplayEffect GetCooldown();

        public AbilitySpec Generate(GASComponent owner, int level)
        {
            return AbilitySpec.Generate(this, owner, level);
        }
    }
}
