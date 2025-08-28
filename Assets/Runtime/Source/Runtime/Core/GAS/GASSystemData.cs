using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public class GASSystemData : ISystemData
    {
        [Header("Identity")] 
        
        public GASIdentityData Identity;
        
        [Header("Ability Components")] 
        
        public EAbilityActivationPolicy ActivationPolicy;
        public int MaxAbilities;
        public List<Ability> StartingAbilities;
        public bool AllowDuplicateAbilities;
        
        [Space(5)]
        
        public List<AbstractImpactWorker> ImpactWorkers;
        
        [Header("Attribute Components")]
        
        public AttributeSet AttributeSet;
        public List<AbstractAttributeChangeEvent> AttributeChangeEvents;
        
        [Header("Tag Workers")]
        
        public List<AbstractTagWorker> TagWorkers;

        public EAbilityActivationPolicy GetActivationPolicy()
        {
            return ActivationPolicy;
        }
        public int GetMaxAbilities()
        {
            return MaxAbilities;
        }
        public List<Ability> GetStartingAbilities()
        {
            return StartingAbilities;
        }
        public bool GetAllowDuplicateAbilities()
        {
            return AllowDuplicateAbilities;
        }
        public List<AbstractImpactWorker> GetImpactWorkers()
        {
            return ImpactWorkers;
        }
        public IAttributeSet GetAttributeSet()
        {
            return AttributeSet;
        }
        public List<AbstractAttributeChangeEvent> GetAttributeChangeEvents()
        {
            return AttributeChangeEvents;
        }
        public List<AbstractTagWorker> GetTagWorkers()
        {
            return TagWorkers;
        }
    }

    public interface ISystemData
    {
        public EAbilityActivationPolicy GetActivationPolicy();
        public List<Ability> GetStartingAbilities();
        public bool GetAllowDuplicateAbilities();
        
        public List<AbstractImpactWorker> GetImpactWorkers();
        
        public IAttributeSet GetAttributeSet();
        public List<AbstractAttributeChangeEvent> GetAttributeChangeEvents();
        
        public List<AbstractTagWorker> GetTagWorkers();

        public static ISystemData GenerateEmpty()
        {
            return new CustomSystemData();
        }
    }

    public class CustomSystemData : ISystemData
    {

        public EAbilityActivationPolicy GetActivationPolicy()
        {
            return EAbilityActivationPolicy.NoRestrictions;
        }
        public int GetMaxAbilities()
        {
            return int.MaxValue;
        }
        public List<Ability> GetStartingAbilities()
        {
            return new List<Ability>();
        }
        public bool GetAllowDuplicateAbilities()
        {
            return false;
        }
        public List<AbstractImpactWorker> GetImpactWorkers()
        {
            return new List<AbstractImpactWorker>();
        }
        public IAttributeSet GetAttributeSet()
        {
            return IAttributeSet.GenerateEmpty();
        }
        public List<AbstractAttributeChangeEvent> GetAttributeChangeEvents()
        {
            return new List<AbstractAttributeChangeEvent>();
        }
        public List<AbstractTagWorker> GetTagWorkers()
        {
            return new List<AbstractTagWorker>();
        }
    }
}
