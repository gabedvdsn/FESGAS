using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(menuName = "FESGAS/GAS System Data", fileName = "GSD_")]
    public class GASSystemData : ScriptableObject, ISystemData
    {
        [Header("Ability Components")] 
        
        public EAbilityActivationPolicy ActivationPolicy;
        public int MaxAbilities;
        public List<AbilityScriptableObject> StartingAbilities;
        public bool AllowDuplicateAbilities;
        
        [Space(5)]
        
        public List<AbstractImpactWorkerScriptableObject> ImpactWorkers;
        
        [Header("Attribute Components")]
        
        public AttributeSetScriptableObject AttributeSet;
        public List<AbstractAttributeChangeEventScriptableObject> AttributeChangeEvents;
        
        [Header("Tag Workers")]
        
        public List<AbstractTagWorkerScriptableObject> TagWorkers;

        public EAbilityActivationPolicy GetActivationPolicy()
        {
            return ActivationPolicy;
        }
        public int GetMaxAbilities()
        {
            return MaxAbilities;
        }
        public List<AbilityScriptableObject> GetStartingAbilities()
        {
            return StartingAbilities;
        }
        public bool GetAllowDuplicateAbilities()
        {
            return AllowDuplicateAbilities;
        }
        public List<AbstractImpactWorkerScriptableObject> GetImpactWorkers()
        {
            return ImpactWorkers;
        }
        public IAttributeSet GetAttributeSet()
        {
            return AttributeSet;
        }
        public List<AbstractAttributeChangeEventScriptableObject> GetAttributeChangeEvents()
        {
            return AttributeChangeEvents;
        }
        public List<AbstractTagWorkerScriptableObject> GetTagWorkers()
        {
            return TagWorkers;
        }
    }

    public interface ISystemData
    {
        public EAbilityActivationPolicy GetActivationPolicy();
        public int GetMaxAbilities();
        public List<AbilityScriptableObject> GetStartingAbilities();
        public bool GetAllowDuplicateAbilities();
        
        public List<AbstractImpactWorkerScriptableObject> GetImpactWorkers();
        
        public IAttributeSet GetAttributeSet();
        public List<AbstractAttributeChangeEventScriptableObject> GetAttributeChangeEvents();
        
        public List<AbstractTagWorkerScriptableObject> GetTagWorkers();

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
        public List<AbilityScriptableObject> GetStartingAbilities()
        {
            return new List<AbilityScriptableObject>();
        }
        public bool GetAllowDuplicateAbilities()
        {
            return false;
        }
        public List<AbstractImpactWorkerScriptableObject> GetImpactWorkers()
        {
            return new List<AbstractImpactWorkerScriptableObject>();
        }
        public IAttributeSet GetAttributeSet()
        {
            return IAttributeSet.GenerateEmpty();
        }
        public List<AbstractAttributeChangeEventScriptableObject> GetAttributeChangeEvents()
        {
            return new List<AbstractAttributeChangeEventScriptableObject>();
        }
        public List<AbstractTagWorkerScriptableObject> GetTagWorkers()
        {
            return new List<AbstractTagWorkerScriptableObject>();
        }
    }
}
