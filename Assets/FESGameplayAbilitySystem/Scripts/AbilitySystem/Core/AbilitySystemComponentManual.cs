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
        
        public int MaxAbilities = 99;
        public List<AbilityScriptableObject> StartingAbilities;

        [Header("Application Workers")] 
        
        public List<AbstractApplicationWorkerScriptableObject> ApplicationWorkers;
        
        [Header("Impact Workers")]
        
        public List<AbstractImpactWorkerScriptableObject> ImpactWorkers;

        public override void Initialize(GASComponentBase system)
        {
            maxAbilities = MaxAbilities;
            applicationWorkers = ApplicationWorkers;
            impactWorkers = ImpactWorkers;
            startingAbilities = StartingAbilities;
            
            base.Initialize(system);
        }

    }
}
