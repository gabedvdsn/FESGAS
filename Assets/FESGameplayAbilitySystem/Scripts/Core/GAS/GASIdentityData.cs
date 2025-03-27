using System;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [Serializable]
    public class GASIdentityData
    {
        public int Level = 1;
        public int MaxLevel = 15;

        public GameplayTagScriptableObject NameTag;

        private GASComponentBase System;

        public void Initialize(GASComponentBase system) => System = system;

        public string DistinctName => NameTag ? NameTag.Name : System is null ? "AnonGAS" : $"AnonGAS-({System.gameObject.name})";
        
        public override string ToString()
        {
            return DistinctName;
        }
    }
}
