using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace FESGameplayAbilitySystem
{
    [Serializable]
    public class AbilityDefinition
    {
        [Header("Information")]
        
        public string Name;
        public string Description;
        
        [Header("Icons")]
        
        public Sprite UnlearnedIcon;
        public Sprite NormalIcon;
        public Sprite QueuedIcon;
        public Sprite OnCooldownIcon;
        
        [Header("Type")]
        
        public EAbilityType Type;
        public bool ActivateImmediately; 
        
    }
}
