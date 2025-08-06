using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AYellowpaper.SerializedCollections;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public class AbilitySystemComponentManual : AbilitySystemComponent
    {
        [Header("Ability System")] 
        
        public EAbilityActivationPolicy ActivationPolicy;
        public List<AbilityScriptableObject> StartingAbilities;
        
        [Header("Impact Workers")]
        
        public List<AbstractImpactWorkerScriptableObject> ImpactWorkers;

        public override void Initialize(GASComponentBase system)
        {
            activationPolicy = ActivationPolicy;
            impactWorkers = ImpactWorkers;
            startingAbilities = StartingAbilities;
            
            base.Initialize(system);
        }

    }
}
