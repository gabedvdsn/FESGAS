using System;
using System.Collections;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public abstract class AbstractAbilityScriptableObject : ScriptableObject
    {
        [Header("Ability")]
        
        public AbilityDefinition Definition;
        public AbilityTags Tags;
        
        [Header("Leveling")]

        public int StartingLevel = 1;
        public int MaxLevel = 4;

        [Header("Using")]
        
        public GameplayEffectScriptableObject Cost;
        public GameplayEffectScriptableObject Cooldown;
        
        public abstract AbstractAbilitySpec Generate(GASComponent Owner, int Level);
        
        public abstract class AbstractAbilitySpec
        {
            public GASComponent System;
            public AbstractAbilityScriptableObject Base;
        
            public int Level;

            protected AbstractAbilitySpec(GASComponent system, AbstractAbilityScriptableObject ability, int level)
            {
                System = system;
                Base = ability;
                Level = level;
            }
            
            public abstract IEnumerator DoAbility();

            public bool CanActivateAbility()
            {
                return false;
            }
        
        }
    }
}
