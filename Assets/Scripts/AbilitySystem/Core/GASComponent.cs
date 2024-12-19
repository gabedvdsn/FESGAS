using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [RequireComponent(typeof(AttributeSystemComponent))]
    public class GASComponent : MonoBehaviour
    {
        [HideInInspector] public AttributeSystemComponent AttributeSystem;
        public AbilitySystemData Data;

        public List<GameplayEffectScriptableObject> Effects;

        private List<GameplayEffectShelfContainer> EffectShelf;
        private List<GameplayEffectShelfContainer> FinishedEffects;
        private bool needsCleaning;

        private void Awake()
        {
            AttributeSystem = GetComponent<AttributeSystemComponent>();

            EffectShelf = new List<GameplayEffectShelfContainer>();
            FinishedEffects = new List<GameplayEffectShelfContainer>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                GameplayEffectSpec spec = GenerateEffectSpec(this, Effects[0], 1);
                ApplyGameplayEffect(spec);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                GameplayEffectSpec spec = GenerateEffectSpec(this, Effects[1], 1);
                ApplyGameplayEffect(spec);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                GameplayEffectSpec spec = GenerateEffectSpec(this, Effects[2], 1);
                ApplyGameplayEffect(spec);
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                GameplayEffectSpec spec = GenerateEffectSpec(this, Effects[3], 1);
                ApplyGameplayEffect(spec);
            }
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                GameplayEffectSpec spec = GenerateEffectSpec(this, Effects[4], 1);
                ApplyGameplayEffect(spec);
            }
            
            TickEffectShelf();
            if (needsCleaning) ClearFinishedEffects();
        }
        
        #region Effect Handling
        
        public GameplayEffectSpec GenerateEffectSpec(GASComponent Source, GameplayEffectScriptableObject GameplayEffect, int Level)
        {
            return GameplayEffect.Generate(Source, this, Level);
        }

        public bool ApplyGameplayEffect(GameplayEffectSpec spec)
        {
            if (spec is null) return false;
            
            Debug.Log($"Applying gameplay effect: {spec.Base.name}");
            
            if (!ValidateEffectApplicationRequirements(spec)) return false;
            
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

        private void ApplyDurationalGameplayEffect(GameplayEffectSpec spec)
        {
            if (!AttributeSystem.TryGetAttributeValue(spec.Base.ImpactSpecification.AttributeTarget, out _)) return;

            GameplayEffectShelfContainer container = new GameplayEffectShelfContainer(spec, true);
            
            EffectShelf.Add(container);
            
            if (spec.Base.DurationSpecification.TickOnApplication)
            {
                ApplyInstantGameplayEffect(spec);
            }
        }

        private void ApplyInstantGameplayEffect(GameplayEffectSpec spec)
        {
            if (!AttributeSystem.TryGetAttributeValue(spec.Base.ImpactSpecification.AttributeTarget, out AttributeValue attributeValue)) return;
            AttributeSystem.ModifyAttribute(spec.Base.ImpactSpecification.AttributeTarget, spec.ToModifiedAttributeValue(attributeValue));
        }

        private void HandleGameplayEffects()
        {
            List<GameplayEffectShelfContainer> toRemove = new List<GameplayEffectShelfContainer>();
            
            foreach (GameplayEffectShelfContainer container in EffectShelf)
            {
                if (ValidateEffectRemovalRequirements(container.Spec))
                {
                    toRemove.Add(container);
                }
                else if (!ValidateEffectOngoingRequirements(container))
                {
                    container.Ongoing = false;
                }
            }

            foreach (GameplayEffectShelfContainer container in toRemove)
            {
                EffectShelf.Remove(container);
            }
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
        
        #endregion

        #region Effect Requirement Validation
        
        /// <summary>
        /// Should the spec be applied?
        /// </summary>
        /// <param name="spec"></param>
        /// <returns></returns>
        private bool ValidateEffectApplicationRequirements(GameplayEffectSpec spec)
        {
            List<GameplayTagScriptableObject> appliedTags = new List<GameplayTagScriptableObject>();
            
            // Collect applied tags
            foreach (GameplayEffectShelfContainer container in EffectShelf) appliedTags.AddRange(container.Spec.Base.GrantedTags);

            // Validate source application requirements
            return spec.Base.TargetRequirements.CheckApplicationRequirements(appliedTags) &&
                   // Validate target application requirements
                   spec.Source.ValidateEffectApplicationRequirements(spec.Base.SourceRequirements);
        }
        
        /// <summary>
        /// Are the application requirements met? (i.e. should the effect be applied?)
        /// </summary>
        /// <param name="requirements"></param>
        /// <returns></returns>
        private bool ValidateEffectApplicationRequirements(GameplayEffectRequirements requirements)
        {
            List<GameplayTagScriptableObject> appliedTags = new List<GameplayTagScriptableObject>();
            
            // Collect applied tags
            foreach (GameplayEffectShelfContainer container in EffectShelf) appliedTags.AddRange(container.Spec.Base.GrantedTags);

            return requirements.CheckApplicationRequirements(appliedTags);
        }
        
        /// <summary>
        /// Should the spec be ongoing?
        /// </summary>
        /// <param name="specContainer"></param>
        /// <returns></returns>
        private bool ValidateEffectOngoingRequirements(GameplayEffectShelfContainer specContainer)
        {
            List<GameplayTagScriptableObject> appliedTags = new List<GameplayTagScriptableObject>();
            
            // Collect applied tags
            foreach (GameplayEffectShelfContainer container in EffectShelf) appliedTags.AddRange(container.Spec.Base.GrantedTags);

            // Validate source application requirements
            return specContainer.Spec.Base.TargetRequirements.CheckOngoingRequirements(appliedTags) &&
                   // Validate target application requirements
                   specContainer.Spec.Source.ValidateEffectOngoingRequirements(specContainer.Spec.Base.SourceRequirements);
        }
        
        /// <summary>
        /// Are the ongoing requirements met? (i.e. should the effect remain ongoing?)
        /// </summary>
        /// <param name="requirements"></param>
        /// <returns></returns>
        private bool ValidateEffectOngoingRequirements(GameplayEffectRequirements requirements)
        {
            List<GameplayTagScriptableObject> appliedTags = new List<GameplayTagScriptableObject>();
            
            // Collect applied tags
            foreach (GameplayEffectShelfContainer container in EffectShelf) appliedTags.AddRange(container.Spec.Base.GrantedTags);

            return requirements.CheckOngoingRequirements(appliedTags);
        }
        
        /// <summary>
        /// Should the spec be removed?
        /// </summary>
        /// <param name="spec"></param>
        /// <returns></returns>
        private bool ValidateEffectRemovalRequirements(GameplayEffectSpec spec)
        {
            List<GameplayTagScriptableObject> appliedTags = new List<GameplayTagScriptableObject>();
            
            // Collect applied tags
            foreach (GameplayEffectShelfContainer container in EffectShelf) appliedTags.AddRange(container.Spec.Base.GrantedTags);
            
            // Validate source application requirements
            return spec.Base.TargetRequirements.CheckRemovalRequirements(appliedTags) ||
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
            List<GameplayTagScriptableObject> appliedTags = new List<GameplayTagScriptableObject>();
            
            // Collect applied tags
            foreach (GameplayEffectShelfContainer container in EffectShelf) appliedTags.AddRange(container.Spec.Base.GrantedTags);
            
            return requirements.CheckRemovalRequirements(appliedTags);
        }
        
        #endregion
        
    }
}