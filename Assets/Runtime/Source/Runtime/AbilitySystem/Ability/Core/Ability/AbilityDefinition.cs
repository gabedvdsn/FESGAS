using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace FESGameplayAbilitySystem
{
    public class AbilityDefinition
    {
        public string Name;
        public string Description;
        
        public EAbilityActivationPolicyExtended ActivationPolicy = EAbilityActivationPolicyExtended.SingleActive; 
        public bool ActivateImmediately;
        
        public Sprite UnlearnedIcon;
        public Sprite NormalIcon;
        public Sprite QueuedIcon;
        public Sprite OnCooldownIcon;
    }
}
