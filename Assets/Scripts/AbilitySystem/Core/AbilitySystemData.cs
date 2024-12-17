using System;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [Serializable]
    public class AbilitySystemData
    {
        public int Level = 1;

        [SerializeField] private string distinctName;

        public string DistinctName
        {
            get => string.IsNullOrEmpty(distinctName) ? "Non-Distinct Ability System" : distinctName;
            set => distinctName = value;
        }
    }
}
