using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public class GASData
    {
        public GASIdentityData Identity;
        
        public EAbilityActivationPolicy ActivationPolicy = EAbilityActivationPolicy.SingleActiveQueue;
        public int MaxAbilities;
        public List<Ability> StartingAbilities = new();
        public bool AllowDuplicateAbilities;
        
        public List<AbstractImpactWorker> ImpactWorkers = new();
        
        public AttributeSet AttributeSet = new();
        public List<AbstractAttributeChangeEvent> AttributeChangeEvents = new();
        
        public List<AbstractTagWorker> TagWorkers = new();
    }

    public class GameRootData : GASData
    {
        public GameRootData()
        {
            MaxAbilities = int.MaxValue;
            ActivationPolicy = EAbilityActivationPolicy.NoRestrictions;
        }
    }
}
