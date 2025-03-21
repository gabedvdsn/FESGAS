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

        private GASComponent System;

        public void Initialize(GASComponent system) => System = system;

        public string DistinctName => NameTag ? (System is null ? "Non-Distinct Ability System" : System.gameObject.name) : NameTag.Name;
        
        public override string ToString()
        {
            return DistinctName;
        }
    }
}
