using System;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.Serialization;

namespace FESGameplayAbilitySystem
{
    [RequireComponent(typeof(AttributeSystemComponent))]
    public class GASComponent : MonoBehaviour
    {
        [HideInInspector] public AttributeSystemComponent AttributeSystem;
        [HideInInspector] public AbilitySystemComponent AbilitySystem;
        public GASComponentData Data;

        public SerializedDictionary<KeyCode, AbilityScriptableObject> StartingAbilityMapping;
        private Dictionary<KeyCode, int> AbilityMap; 

        private List<GameplayEffectShelfContainer> EffectShelf;
        private List<GameplayEffectShelfContainer> FinishedEffects;
        private bool needsCleaning;

        private void Awake()
        {
            AttributeSystem = GetComponent<AttributeSystemComponent>();
            AbilitySystem = GetComponent<AbilitySystemComponent>();

            EffectShelf = new List<GameplayEffectShelfContainer>();
            FinishedEffects = new List<GameplayEffectShelfContainer>();

            
        }

        private void Start()
        {
            InitializeAbilities();
        }

        private void Update()
        {
            HandleInput();
            
            TickEffectShelf();
            if (needsCleaning) ClearFinishedEffects();
        }

        private void InitializeAbilities()
        {
            AbilityMap = new Dictionary<KeyCode, int>();
            foreach (KeyCode key in StartingAbilityMapping.Keys)
            {
                if (StartingAbilityMapping[key] is null)
                {
                    AbilityMap[key] = -1;
                    continue;
                }
                AbilitySystem.GiveAbility(StartingAbilityMapping[key], 1, out int abilityIndex);
                AbilityMap[key] = abilityIndex;
            }
        }

        private void HandleInput()
        {
            foreach (KeyCode keyCode in AbilityMap.Keys)
            {
                if (Input.GetKeyDown(keyCode)) AbilitySystem.ActivateAbilityAtIndex(AbilityMap[keyCode]);
            }
        }
        
        #region Effect Handling
        
        public GameplayEffectSpec GenerateEffectSpec(GASComponent Source, GameplayEffectScriptableObject GameplayEffect, int Level)
        {
            return GameplayEffect.Generate(Source, this, Level);
        }

        public GameplayEffectSpec GenerateEffectSpec(AbilitySpec ability, GameplayEffectScriptableObject GameplayEffect)
        {
            return GameplayEffect.Generate(ability, this);
        }

        public bool ApplyGameplayEffect(GameplayEffectSpec spec)
        {
            if (spec is null) return false;
            
            
            if (!ValidateEffectApplicationRequirements(spec)) return false;
            
            Debug.Log($"Applying gameplay effect: {spec.Base.name} ({spec.Level}) for {spec.Base.ImpactSpecification.GetMagnitude(spec)} {spec.Base.ImpactSpecification.AttributeTarget.Name} ({spec.Base.DurationSpecification.DurationPolicy})");
            
            switch (spec.Base.DurationSpecification.DurationPolicy)
            {

                case GameplayEffectDurationPolicy.Instant:
                    ApplyInstantGameplayEffect(spec);
                    break;
                case GameplayEffectDurationPolicy.Infinite:
                case GameplayEffectDurationPolicy.Durational:
                    ApplyDurationalGameplayEffect(spec);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            HandleGameplayEffects();

            return true;
        }

        public bool ApplyGameplayEffect(AbilitySpec ability, GameplayEffectScriptableObject GameplayEffect)
        {
            GameplayEffectSpec spec = GenerateEffectSpec(ability, GameplayEffect);
            return ApplyGameplayEffect(spec);
        }

        public void RemoveGameplayEffect(GameplayEffectScriptableObject GameplayEffect)
        {
            Debug.Log($"Removing {GameplayEffect.Identifier.Name}");
            List<GameplayEffectShelfContainer> toRemove = EffectShelf.Where(container => container.Spec.Base == GameplayEffect).ToList();
            foreach (GameplayEffectShelfContainer container in toRemove)
            {
                container.Valid = false;
                EffectShelf.Remove(container);
            }
        }

        private void ApplyDurationalGameplayEffect(GameplayEffectSpec spec)
        {
            if (!AttributeSystem.TryGetAttributeValue(spec.Base.ImpactSpecification.AttributeTarget, out _)) return;

            GameplayEffectShelfContainer container = new GameplayEffectShelfContainer(spec, ValidateEffectOngoingRequirements(spec));

            EffectDebugManager.Instance.CreateDebugFor(ref container);
            
            EffectShelf.Add(container);
            
            if (spec.Base.DurationSpecification.TickOnApplication)
            {
                ApplyInstantGameplayEffect(spec);
            }
        }

        private void ApplyInstantGameplayEffect(GameplayEffectSpec spec)
        {
            if (!AttributeSystem.TryGetAttributeValue(spec.Base.ImpactSpecification.AttributeTarget, out AttributeValue attributeValue)) return;
            //Debug.Log(spec.ToModifiedAttributeValue(attributeValue));
            AttributeSystem.ModifyAttribute(spec.Base.ImpactSpecification.AttributeTarget, spec.ToModifiedAttributeValue(attributeValue));
        }

        private void HandleGameplayEffects()
        {
            List<GameplayEffectShelfContainer> toRemove = new List<GameplayEffectShelfContainer>();
            
            // Validate removal and ongoing requirements
            foreach (GameplayEffectShelfContainer container in EffectShelf)
            {
                if (ValidateEffectRemovalRequirements(container.Spec)) toRemove.Add(container);
                else container.Ongoing = ValidateEffectOngoingRequirements(container.Spec);
            }

            // Remove containers 
            foreach (GameplayEffectShelfContainer container in toRemove) EffectShelf.Remove(container);
        }

        private void TickEffectShelf()
        {
            foreach (GameplayEffectShelfContainer container in EffectShelf)
            {
                GameplayEffectDurationPolicy durationPolicy = container.Spec.Base.DurationSpecification.DurationPolicy;
                
                if (durationPolicy == GameplayEffectDurationPolicy.Instant) continue;
                
                container.TickPeriodic(Time.deltaTime, out bool executeEffect);
                if (executeEffect && container.Ongoing) ApplyInstantGameplayEffect(container.Spec);

                if (durationPolicy == GameplayEffectDurationPolicy.Infinite) continue;
                
                container.UpdateTimeRemaining(Time.deltaTime);

                if (container.DurationRemaining > 0) continue;
                
                FinishedEffects.Add(container);
                needsCleaning = true;
            }
        }

        private void ClearFinishedEffects()
        {
            //EffectShelf.RemoveAll(container => container.DurationRemaining <= 0 && container.Spec.Base.DurationSpecification.DurationPolicy != GameplayEffectDurationPolicy.Infinite);
            foreach (GameplayEffectShelfContainer container in FinishedEffects) EffectShelf.Remove(container);
            FinishedEffects.Clear();
            
            needsCleaning = false;
        }

        public List<GameplayTagScriptableObject> GetAppliedTags()
        {
            List<GameplayTagScriptableObject> appliedTags = new List<GameplayTagScriptableObject>();
            
            // Collect applied tags
            foreach (GameplayEffectShelfContainer container in EffectShelf) appliedTags.AddRange(container.Spec.Base.GrantedTags);

            return appliedTags;
        }
        
        #endregion

        #region Effect Requirement Validation
        
        /// <summary>
        /// Should the spec be applied?
        /// </summary>
        /// <param name="spec"></param>
        /// <returns></returns>
        private bool ValidateEffectApplicationRequirements(GameplayEffectSpec spec)
        {
            List<GameplayTagScriptableObject> appliedTags = GetAppliedTags();

            // Validate target (this) application requirements
            return spec.Base.TargetRequirements.CheckApplicationRequirements(appliedTags) &&
                   !spec.Base.TargetRequirements.CheckRemovalRequirements(appliedTags) &&
                   // Validate source application requirements
                   spec.Source.ValidateEffectApplicationRequirements(spec.Base.SourceRequirements) &&
                   !spec.Source.ValidateEffectRemovalRequirements(spec.Base.SourceRequirements);
        }
        
        /// <summary>
        /// Are the application requirements met? (i.e. should the effect be applied?)
        /// </summary>
        /// <param name="requirements"></param>
        /// <returns></returns>
        private bool ValidateEffectApplicationRequirements(GameplayEffectRequirements requirements)
        {
            return requirements.CheckApplicationRequirements(GetAppliedTags());
        }
        
        /// <summary>
        /// Should the spec be ongoing?
        /// </summary>
        /// <param name="spec"></param>
        /// <returns></returns>
        private bool ValidateEffectOngoingRequirements(GameplayEffectSpec spec)
        {
            // Validate source application requirements
            return spec.Base.TargetRequirements.CheckOngoingRequirements(GetAppliedTags()) &&
                   // Validate target application requirements
                   spec.Source.ValidateEffectOngoingRequirements(spec.Base.SourceRequirements);
        }
        
        /// <summary>
        /// Are the ongoing requirements met? (i.e. should the effect remain ongoing?)
        /// </summary>
        /// <param name="requirements"></param>
        /// <returns></returns>
        private bool ValidateEffectOngoingRequirements(GameplayEffectRequirements requirements)
        {
            return requirements.CheckOngoingRequirements(GetAppliedTags());
        }
        
        /// <summary>
        /// Should the spec be removed?
        /// </summary>
        /// <param name="spec"></param>
        /// <returns></returns>
        private bool ValidateEffectRemovalRequirements(GameplayEffectSpec spec)
        {
            // Validate source application requirements
            return spec.Base.TargetRequirements.CheckRemovalRequirements(GetAppliedTags()) ||
                   // Validate target application requirements
                   spec.Source.ValidateEffectRemovalRequirements(spec.Base.SourceRequirements);
        }

        /// <summary>
        /// Are the removal requirements met? (i.e. should the effect be removed?)
        /// </summary>
        /// <param name="requirements"></param>
        /// <returns></returns>
        private bool ValidateEffectRemovalRequirements(GameplayEffectRequirements requirements)
        {
            
            return requirements.CheckRemovalRequirements(GetAppliedTags());
        }
        
        #endregion
        
        #region Effect Helpers

        public GameplayEffectDuration GetLongestDurationFor(GameplayTagScriptableObject[] lookForTags)
        {
            float longestDuration = float.MinValue;
            float longestRemaining = float.MinValue;
            foreach (GameplayEffectShelfContainer container in EffectShelf)
            {
                foreach (GameplayTagScriptableObject specTag in container.Spec.Base.GrantedTags)
                {
                    if (!lookForTags.Contains(specTag)) continue;
                    if (container.Spec.Base.DurationSpecification.DurationPolicy == GameplayEffectDurationPolicy.Infinite)
                    {
                        return new GameplayEffectDuration(float.MaxValue, float.MaxValue);
                    }

                    if (!(container.TotalDuration > longestDuration)) continue;
                    longestDuration = container.TotalDuration;
                    longestRemaining = container.DurationRemaining;
                }
            }

            return new GameplayEffectDuration(longestDuration, longestRemaining);
        }
        
        #endregion
    }
}