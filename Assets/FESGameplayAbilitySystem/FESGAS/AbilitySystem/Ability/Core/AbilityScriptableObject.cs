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

        public int StartingLevel = 1;
        public int MaxLevel = 4;
        public bool IgnoreWhenLevelZero = true;

        [Header("Using")]
        
        public GameplayEffectScriptableObject Cost;
        public GameplayEffectScriptableObject Cooldown;

        public AbilitySpec Generate(GASComponentBase owner, int level)
        {
            return AbilitySpec.Generate(this, owner, level);
        }

        public override string ToString()
        {
            return Tags.AssetTag.Name;
        }
    }

    public enum EAbilityType
    {
        Activated,
        AlwaysActive,
        Toggled
    }

}
