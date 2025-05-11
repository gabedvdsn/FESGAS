using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/*
 * TODO
 * GEs with conditions
 * Passives & aura abilities
 * GAS ability behavior
 */

namespace FESGameplayAbilitySystem
{
    [RequireComponent(typeof(AttributeSystemComponent))]
    [RequireComponent(typeof(AbilitySystemComponent))]
    public class GASComponent : GASComponentBase
    {
        
        public GASSystemData SystemData;

        protected override void PrepareSystem()
        {
            TagCache = new TagCache(this, SystemData.TagWorkers);
            
            AttributeSystem.ProvidePrerequisiteData(SystemData);
            AbilitySystem.ProvidePrerequisiteData(SystemData);
        }
    }
}
