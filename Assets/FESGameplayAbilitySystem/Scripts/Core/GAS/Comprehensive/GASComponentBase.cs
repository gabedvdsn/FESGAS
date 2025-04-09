using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace FESGameplayAbilitySystem
{
    public abstract class GASComponentBase : MonoBehaviour
    {
        [Header("Gameplay Ability System")]
        
        public GASIdentityData Identity;
        
        [HideInInspector] public AttributeSystemComponent AttributeSystem;
        [HideInInspector] public AbilitySystemComponent AbilitySystem;
        
        private List<AbstractGameplayEffectShelfContainer> EffectShelf;
        private List<AbstractGameplayEffectShelfContainer> FinishedEffects;
        private bool needsCleaning;

        public TagCache TagCache;
        
        protected void Awake()
        {
            AttributeSystem = GetComponent<AttributeSystemComponent>();
            AbilitySystem = GetComponent<AbilitySystemComponent>();
            
            EffectShelf = new List<AbstractGameplayEffectShelfContainer>();
            FinishedEffects = new List<AbstractGameplayEffectShelfContainer>();
            
            Identity.Initialize(this);
            
            PrepareSystem();
            
            AttributeSystem.Initialize(this);
            AbilitySystem.Initialize(this);
        }

        protected abstract void PrepareSystem();
        
        private void Update()
        {
            TickEffectShelf();
        }

        private void FinishFrame()
        {
            if (needsCleaning) ClearFinishedEffects();
            
            TagCache.TickTagWorkers();
        }
        
        #region Effect Handling
        
        public GameplayEffectSpec GenerateEffectSpec(IEffectDerivation derivation, GameplayEffectScriptableObject GameplayEffect)
        {
            return GameplayEffect.Generate(derivation, this);
        }

        public bool ApplyGameplayEffect(GameplayEffectSpec spec)
        {
            if (spec is null) return false;
            
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
            
            foreach (var containedEffect in spec.Base.ImpactSpecification.GetContainedEffects(EApplyDuringRemove.OnApply))
            {
                ApplyGameplayEffect(spec.Derivation, containedEffect);
            }
            
            HandleGameplayEffects();

            return true;
        }

        /// <summary>
         /// Applies a gameplay effect to the container with respect to a derivation (effect) that is already applied
         /// </summary>
         /// <param name="derivation"></param>
         /// <param name="GameplayEffect"></param>
         /// <returns></returns>
        public bool ApplyGameplayEffect(IEffectDerivation derivation, GameplayEffectScriptableObject GameplayEffect)
        {
            GameplayEffectSpec spec = GenerateEffectSpec(derivation, GameplayEffect);
            return ApplyGameplayEffect(spec);
        }

        public void RemoveGameplayEffect(GameplayEffectScriptableObject GameplayEffect)
        {
            List<AbstractGameplayEffectShelfContainer> toRemove = EffectShelf.Where(container => container.Spec.Base == GameplayEffect).ToList();
            foreach (AbstractGameplayEffectShelfContainer container in toRemove)
            {
                FinishedEffects.Add(container);
                needsCleaning = true;
            }
        }

        /// <summary>
        /// Applies a durational/infinite gameplay effect to the component
        /// </summary>
        /// <param name="spec"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void ApplyDurationalGameplayEffect(GameplayEffectSpec spec)
        {
            if (!AttributeSystem.DefinesAttribute(spec.Base.ImpactSpecification.AttributeTarget)) return;

            if (TryHandleExistingDurationalGameplayEffect(spec)) return;

            AbstractGameplayEffectShelfContainer container;
            switch (spec.Base.ImpactSpecification.ReApplicationPolicy)
            {
                case EGameplayEffectApplicationPolicy.Refresh:
                case EGameplayEffectApplicationPolicy.Extend:
                case EGameplayEffectApplicationPolicy.Append:
                    container = GameplayEffectShelfContainer.Generate(spec, ValidateEffectOngoingRequirements(spec));
                    break;
                case EGameplayEffectApplicationPolicy.Stack:
                case EGameplayEffectApplicationPolicy.StackRefresh:
                case EGameplayEffectApplicationPolicy.StackExtend:
                    container = StackableGameplayShelfContainer.Generate(spec, ValidateEffectOngoingRequirements(spec));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
                
            EffectDebugManager.Instance.CreateDebugFor(ref container);
            TagsDebugManager.Instance.CreateDebugFor(ref container);
            
            EffectShelf.Add(container);
            TagCache.AddTaggable(container);
            
            container.RunEffectApplicationWorkers();
                
            if (spec.Base.DurationSpecification.TickOnApplication)
            {
                ApplyInstantGameplayEffect(container);
            }
        }
        
        /// <summary>
        /// Instantly applies the effects of an instant gameplay effect
        /// </summary>
        /// <param name="spec"></param>
        private void ApplyInstantGameplayEffect(GameplayEffectSpec spec)
        {
            if (!AttributeSystem.TryGetAttributeValue(spec.Base.ImpactSpecification.AttributeTarget, out AttributeValue attributeValue)) return;

            // Apply tags
            TagCache.AddTaggable(spec);
            
            SourcedModifiedAttributeValue sourcedModifiedValue = spec.SourcedImpact(attributeValue);
            sourcedModifiedValue = spec.Source.AbilitySystem.ApplyApplicationModifications(this, sourcedModifiedValue);
            
            AttributeSystem.ModifyAttribute(spec.Base.ImpactSpecification.AttributeTarget, sourcedModifiedValue);
            
            spec.RunEffectApplicationWorkers();
            spec.RunEffectRemovalWorkers();
            
            HandleGameplayEffects();
            TagCache.RemoveTaggable(spec);
        }
        
        /// <summary>
        /// Instantly applies the effects of a durational/infinite gameplay effect
        /// </summary>
        /// <param name="container"></param>
        private void ApplyInstantGameplayEffect(AbstractGameplayEffectShelfContainer container)
        {
            if (!AttributeSystem.TryGetAttributeValue(container.Spec.Base.ImpactSpecification.AttributeTarget, out AttributeValue attributeValue)) return;
            
            SourcedModifiedAttributeValue sourcedModifiedValue = container.Spec.SourcedImpact(container, attributeValue);
            sourcedModifiedValue = container.Spec.Source.AbilitySystem.ApplyApplicationModifications(this, sourcedModifiedValue);
            
            AttributeSystem.ModifyAttribute(container.Spec.Base.ImpactSpecification.AttributeTarget, sourcedModifiedValue);
            
            container.RunEffectApplicationWorkers();
            
            foreach (var containedEffect in container.Spec.Base.ImpactSpecification.GetContainedEffects(EApplyDuringRemove.OnTick))
            {
                ApplyGameplayEffect(container.Spec.Derivation, containedEffect);
            }
        }
        
        private bool TryHandleExistingDurationalGameplayEffect(GameplayEffectSpec spec)
        {
            if (!TryGetEffectContainer(spec.Base, out AbstractGameplayEffectShelfContainer container)) return false;
            
            switch (spec.Base.ImpactSpecification.ReApplicationPolicy)
            {
                case EGameplayEffectApplicationPolicy.Refresh:
                    container.Refresh();
                    return true;
                case EGameplayEffectApplicationPolicy.Extend:
                    container.Extend(spec.Base.DurationSpecification.GetTotalDuration(spec));
                    return true;
                case EGameplayEffectApplicationPolicy.Append:
                    return false;
                case EGameplayEffectApplicationPolicy.Stack:
                    container.Stack();
                    return true;
                case EGameplayEffectApplicationPolicy.StackRefresh:
                    container.Refresh();
                    return true;
                case EGameplayEffectApplicationPolicy.StackExtend:
                    container.Extend(spec.Base.DurationSpecification.GetTotalDuration(spec));
                    return true;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void HandleGameplayEffects()
        {
            // Validate removal and ongoing requirements
            foreach (AbstractGameplayEffectShelfContainer container in EffectShelf)
            {
                if (ValidateEffectRemovalRequirements(container.Spec))
                {
                    FinishedEffects.Add(container);
                    needsCleaning = true;
                }
                else
                {
                    container.Ongoing = ValidateEffectOngoingRequirements(container.Spec);
                }
            }
        }

        public void AddTags(IEnumerable<GameplayTagScriptableObject> tags, bool noDuplicates = false)
        {
            foreach (GameplayTagScriptableObject _tag in tags) TagCache.AddTag(_tag, noDuplicates);
            HandleGameplayEffects();
        }

        public void RemoveTags(IEnumerable<GameplayTagScriptableObject> tags)
        {
            foreach (GameplayTagScriptableObject _tag in tags) TagCache.RemoveTag(_tag);
        }

        private void TickEffectShelf()
        {
            foreach (AbstractGameplayEffectShelfContainer container in EffectShelf)
            {
                GameplayEffectDurationPolicy durationPolicy = container.Spec.Base.DurationSpecification.DurationPolicy;
                
                if (durationPolicy == GameplayEffectDurationPolicy.Instant) continue;
                
                float deltaTime = Time.deltaTime;
                
                container.TickPeriodic(deltaTime, out int executeTicks);
                if (executeTicks > 0 && container.Ongoing)
                {
                    for (int _ = 0; _ < executeTicks; _++)
                    {
                        ApplyInstantGameplayEffect(container);
                    }
                }

                if (durationPolicy == GameplayEffectDurationPolicy.Infinite) continue;
                
                container.UpdateTimeRemaining(deltaTime);

                if (container.DurationRemaining > 0) continue;
                
                FinishedEffects.Add(container);
                needsCleaning = true;
            }
        }

        public void AttributeSystemFinished()
        {
            FinishFrame();
        }

        private void ClearFinishedEffects()
        {
            //EffectShelf.RemoveAll(container => container.DurationRemaining <= 0 && container.Spec.Base.DurationSpecification.DurationPolicy != GameplayEffectDurationPolicy.Infinite);
            foreach (AbstractGameplayEffectShelfContainer container in FinishedEffects)
            {
                container.OnRemove();
                EffectShelf.Remove(container);
                container.RunEffectRemovalWorkers();
                
                TagCache.RemoveTaggable(container);
                
                if (container.RetainAttributeImpact()) AttributeSystem.RemoveAttributeDerivation(container);
            }
            
            FinishedEffects.Clear();
            needsCleaning = false;
            
            HandleGameplayEffects();
        }

        public List<GameplayTagScriptableObject> GetAppliedTags()
        {
            return TagCache.GetAppliedTags();
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

        public bool TryGetEffectContainer(GameplayEffectScriptableObject effectBase, out AbstractGameplayEffectShelfContainer container)
        {
            foreach (AbstractGameplayEffectShelfContainer _container in EffectShelf.Where(_container => _container.Spec.Base == effectBase))
            {
                container = _container;
                return true;
            }

            container = null;
            return false;
        }
        
        public GameplayEffectDuration GetLongestDurationFor(GameplayTagScriptableObject[] lookForTags)
        {
            float longestDuration = float.MinValue;
            float longestRemaining = float.MinValue;
            foreach (AbstractGameplayEffectShelfContainer container in EffectShelf)
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

        public override string ToString()
        {
            return $"{Identity}";
        }
    }
}
