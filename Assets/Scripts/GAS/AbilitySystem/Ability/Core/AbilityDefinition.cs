using System;
using UnityEngine;

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
    }
}
