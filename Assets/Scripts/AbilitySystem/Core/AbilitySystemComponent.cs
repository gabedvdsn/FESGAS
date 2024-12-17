using System;
using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [RequireComponent(typeof(AttributeSystemComponent))]
    public class AbilitySystemComponent : MonoBehaviour
    {
        [HideInInspector] public AttributeSystemComponent AttributeSystem;
        public AbilitySystemData Data;

        public List<GameplayEffectScriptableObject> Effects;

        private List<GameplayEffectShelfContainer> EffectShelf;

        private void Awake()
        {
            AttributeSystem = GetComponent<AttributeSystemComponent>();

            EffectShelf = new List<GameplayEffectShelfContainer>();
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
        }
        
        #region Effect Handling
        
        public GameplayEffectSpec GenerateEffectSpec(AbilitySystemComponent Source, GameplayEffectScriptableObject GameplayEffect, int Level)
        {
            return GameplayEffect.Generate(Source, this, Level);
        }

        public bool ApplyGameplayEffect(GameplayEffectSpec spec)
        {
            if (spec is null) return false;
            
            if (!ValidateEffectApplicationRequirements(spec)) return false;
            
            switch (spec.Base.PolicySpecification.DurationPolicy)
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
            
        }

        private void ApplyInstantGameplayEffect(GameplayEffectSpec spec)
        {
            if (!AttributeSystem.TryGetAttributeValue())
            
            float magnitude = spec.Base.ImpactSpecification.GetMagnitude(spec);

            switch (spec.Base.ImpactSpecification.ImpactOperation)
            {

                case CalculationOperation.Add:
                    break;
                case CalculationOperation.Multiply:
                    break;
                case CalculationOperation.Override:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
        }

        private void HandleGameplayEffects()
        {
            
        }
        
        #endregion

        #region Effect Requirement Validation
        
        private bool ValidateEffectApplicationRequirements(GameplayEffectSpec spec)
        {
            List<GameplayTagScriptableObject> appliedTags = new List<GameplayTagScriptableObject>();
            
            // Collect applied tags
            foreach (GameplayEffectShelfContainer container in EffectShelf) appliedTags.AddRange(container.Spec.Base.GrantedTags);

            // Validate source application requirements
            return spec.Base.TargetRequirements.ValidateApplicationRequirements(appliedTags) &&
                   // Validate target application requirements
                   spec.Source.ValidateEffectApplicationRequirements(spec.Base.SourceRequirements);
        }

        private bool ValidateEffectApplicationRequirements(GameplayEffectRequirements requirements)
        {
            List<GameplayTagScriptableObject> appliedTags = new List<GameplayTagScriptableObject>();
            
            // Collect applied tags
            foreach (GameplayEffectShelfContainer container in EffectShelf) appliedTags.AddRange(container.Spec.Base.GrantedTags);

            return requirements.ValidateApplicationRequirements(appliedTags);
        }
        
        private bool ValidateEffectOngoingRequirements(GameplayEffectShelfContainer specContainer)
        {
            List<GameplayTagScriptableObject> appliedTags = new List<GameplayTagScriptableObject>();
            
            // Collect applied tags
            foreach (GameplayEffectShelfContainer container in EffectShelf) appliedTags.AddRange(container.Spec.Base.GrantedTags);

            // Validate source application requirements
            return specContainer.Spec.Base.TargetRequirements.ValidateOngoingRequirements(appliedTags) &&
                   // Validate target application requirements
                   specContainer.Spec.Source.ValidateEffectApplicationRequirements(specContainer.Spec.Base.SourceRequirements);
        }

        private bool ValidateEffectOngoingRequirements(GameplayEffectRequirements requirements)
        {
            List<GameplayTagScriptableObject> appliedTags = new List<GameplayTagScriptableObject>();
            
            // Collect applied tags
            foreach (GameplayEffectShelfContainer container in EffectShelf) appliedTags.AddRange(container.Spec.Base.GrantedTags);

            return requirements.ValidateOngoingRequirements(appliedTags);
        }
        
        private bool ValidateEffectRemovalRequirements(GameplayEffectSpec spec)
        {
            List<GameplayTagScriptableObject> appliedTags = new List<GameplayTagScriptableObject>();
            
            // Collect applied tags
            foreach (GameplayEffectShelfContainer container in EffectShelf) appliedTags.AddRange(container.Spec.Base.GrantedTags);

            // Validate source application requirements
            return spec.Base.TargetRequirements.ValidateRemovalRequirements(appliedTags) &&
                   // Validate target application requirements
                   spec.Source.ValidateEffectApplicationRequirements(spec.Base.SourceRequirements);
        }

        private bool ValidateEffectRemovalRequirements(GameplayEffectRequirements requirements)
        {
            List<GameplayTagScriptableObject> appliedTags = new List<GameplayTagScriptableObject>();
            
            // Collect applied tags
            foreach (GameplayEffectShelfContainer container in EffectShelf) appliedTags.AddRange(container.Spec.Base.GrantedTags);

            return requirements.ValidateRemovalRequirements(appliedTags);
        }
        
        #endregion
        
    }
}