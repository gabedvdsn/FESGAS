using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace FESGameplayAbilitySystem
{
    public class Ability : IHasReadableDefinition
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

        public AbilitySpec Generate(ISource owner, int level = 1)
        {
            return new AbilitySpec(owner, this, level);
        }

        public string GetName()
        {
            return Definition.Name;
        }
        
        public string GetDescription()
        {
            return Definition.Description;
        }
        public Sprite GetPrimaryIcon()
        {
            return Definition.NormalIcon;
        }

        public override string ToString()
        {
            return Tags.AssetTag.GetName();
        }
    }
}
