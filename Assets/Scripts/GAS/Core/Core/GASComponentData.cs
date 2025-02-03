using System;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [Serializable]
    public class GASComponentData
    {
        public int Level = 1;
        public int MaxLevel = 15;

        public GameplayTagScriptableObject NameTag;

        public string DistinctName => NameTag ? "Non-Distinct Ability System" : NameTag.Name;
    }
}
