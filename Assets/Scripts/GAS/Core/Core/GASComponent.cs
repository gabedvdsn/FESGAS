using System;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.Serialization;

namespace FESGameplayAbilitySystem
{
    [RequireComponent(typeof(AttributeSystemComponent))]
    [RequireComponent(typeof(AbilitySystemComponent))]
    public class GASComponent : MonoBehaviour
    {
        [Header("GAS Data")]
        [HideInInspector] public AttributeSystemComponent AttributeSystem;
        [HideInInspector] public AbilitySystemComponent AbilitySystem;
        public GASComponentData Data;
        
        [Header("Tag Workers")]
        
        public List<AbstractTagWorkerScriptableObject> TagWorkers;
        
        [Header("TESTING")]

        public SerializedDictionary<KeyCode, AbilityScriptableObject> StartingAbilityMapping;
        private Dictionary<KeyCode, int> AbilityMap; 

        private List<AbstractGameplayEffectShelfContainer> EffectShelf;
        private List<AbstractGameplayEffectShelfContainer> FinishedEffects;
        private bool needsCleaning;

        public TagCache TagCache;

        private void Awake()
        {
            InitializeSystem();
            
            EffectShelf = new List<AbstractGameplayEffectShelfContainer>();
            FinishedEffects = new List<AbstractGameplayEffectShelfContainer>();

            TagCache = new TagCache(this, TagWorkers);
            
            Data.Initialize(this);
        }

        private void InitializeSystem()
        {
            AttributeSystem = GetComponent<AttributeSystemComponent>();
            AbilitySystem = GetComponent<AbilitySystemComponent>();
            
            AbilitySystem.Initialize(this);
            AttributeSystem.Initialize(this);
            
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
                if (Input.GetKeyDown(keyCode)) AbilitySystem.TryActivateAbility(AbilityMap[keyCode]);
            }
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
            
            // Debug.Log($"Applying gameplay effect: {spec.Base.name} ({spec.Level}) for {spec.Base.ImpactSpecification.GetMagnitude(spec)} {spec.Base.ImpactSpecification.AttributeTarget.Name} ({spec.Base.DurationSpecification.DurationPolicy})");
            
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

        public bool ApplyGameplayEffect(IEffectDerivation derivation, GameplayEffectScriptableObject GameplayEffect)
        {
            GameplayEffectSpec spec = GenerateEffectSpec(derivation, GameplayEffect);
            return ApplyGameplayEffect(spec);
        }

        public void RemoveGameplayEffect(GameplayEffectScriptableObject GameplayEffect)
        {
            Debug.Log($"Removing {GameplayEffect.Identifier.Name}");
            List<AbstractGameplayEffectShelfContainer> toRemove = EffectShelf.Where(container => container.Spec.Base == GameplayEffect).ToList();
            foreach (AbstractGameplayEffectShelfContainer container in toRemove)
            {
                FinishedEffects.Add(container);
                needsCleaning = true;
            }
        }

        private void ApplyDurationalGameplayEffect(GameplayEffectSpec spec)
        {
            if (!AttributeSystem.DefinesAttribute(spec.Base.ImpactSpecification.AttributeTarget)) return;

            if (TryHandleExistingDurationalGameplayEffect(spec)) return;

            AbstractGameplayEffectShelfContainer container;
            switch (spec.Base.ImpactSpecification.ReApplicationPolicy)
            {
                case GameplayEffectApplicationPolicy.Refresh:
                case GameplayEffectApplicationPolicy.Extend:
                case GameplayEffectApplicationPolicy.Append:
                    container = GameplayEffectShelfContainer.Generate(spec, ValidateEffectOngoingRequirements(spec));
                    break;
                case GameplayEffectApplicationPolicy.Stack:
                case GameplayEffectApplicationPolicy.StackRefresh:
                case GameplayEffectApplicationPolicy.StackExtend:
                    container = StackableGameplayShelfContainer.Generate(spec, ValidateEffectOngoingRequirements(spec));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
                
            EffectDebugManager.Instance.CreateDebugFor(ref container);
            TagsDebugManager.Instance.CreateDebugFor(ref container);
            
            EffectShelf.Add(container);
                
            if (spec.Base.DurationSpecification.TickOnApplication)
            {
                ApplyInstantGameplayEffect(container);
            }
        }

        private bool TryHandleExistingDurationalGameplayEffect(GameplayEffectSpec spec)
        {
            if (!TryGetEffectContainer(spec.Base, out AbstractGameplayEffectShelfContainer container)) return false;
            
            switch (spec.Base.ImpactSpecification.ReApplicationPolicy)
            {
                case GameplayEffectApplicationPolicy.Refresh:
                    container.Refresh();
                    return true;
                case GameplayEffectApplicationPolicy.Extend:
                    container.Extend(spec.Base.DurationSpecification.GetTotalDuration(spec));
                    return true;
                case GameplayEffectApplicationPolicy.Append:
                    return false;
                case GameplayEffectApplicationPolicy.Stack:
                    container.Stack();
                    return true;
                case GameplayEffectApplicationPolicy.StackRefresh:
                    container.Refresh();
                    container.Stack();
                    return true;
                case GameplayEffectApplicationPolicy.StackExtend:
                    container.Extend(spec.Base.DurationSpecification.GetTotalDuration(spec));
                    container.Stack();
                    return true;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ApplyInstantGameplayEffect(GameplayEffectSpec spec)
        {
            if (!AttributeSystem.TryGetAttributeValue(spec.Base.ImpactSpecification.AttributeTarget, out AttributeValue attributeValue)) return;

            // Apply tags
            TagCache.AddTaggable(spec);
            
            SourcedModifiedAttributeValue sourcedModifiedValue = spec.SourcedImpact(attributeValue);
            sourcedModifiedValue = spec.Source.AbilitySystem.ApplyApplicationModifications(this, sourcedModifiedValue);
            
            AttributeSystem.ModifyAttribute(spec.Base.ImpactSpecification.AttributeTarget, sourcedModifiedValue);
            if (spec.Base.ImpactSpecification.ContainedEffect)
            {
                ApplyGameplayEffect(spec.Derivation, spec.Base.ImpactSpecification.ContainedEffect);
            }
        }
        
        private void ApplyInstantGameplayEffect(AbstractGameplayEffectShelfContainer container)
        {
            if (!AttributeSystem.TryGetAttributeValue(container.Spec.Base.ImpactSpecification.AttributeTarget, out AttributeValue attributeValue)) return;

            // Apply tags
            TagCache.AddTaggable(container`);
            
            SourcedModifiedAttributeValue sourcedModifiedValue = container.Spec.SourcedImpact(container, attributeValue);
            sourcedModifiedValue = container.Spec.Source.AbilitySystem.ApplyApplicationModifications(this, sourcedModifiedValue);
            
            AttributeSystem.ModifyAttribute(container.Spec.Base.ImpactSpecification.AttributeTarget, sourcedModifiedValue);
            if (container.Spec.Base.ImpactSpecification.ContainedEffect)
            {
                ApplyGameplayEffect(container.Spec.Derivation, container.Spec.Base.ImpactSpecification.ContainedEffect);
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
                    // Debug.Log($"{container} {ValidateEffectOngoingRequirements(container.Spec)}");
                    container.Ongoing = ValidateEffectOngoingRequirements(container.Spec);
                }
            }
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
                    for (int _ = 0; _ < executeTicks; _++) ApplyInstantGameplayEffect(container);
                }

                if (durationPolicy == GameplayEffectDurationPolicy.Infinite) continue;
                
                container.UpdateTimeRemaining(deltaTime);

                if (container.DurationRemaining > 0) continue;
                
                FinishedEffects.Add(container);
                needsCleaning = true;
            }
        }

        private void ClearFinishedEffects()
        {
            //EffectShelf.RemoveAll(container => container.DurationRemaining <= 0 && container.Spec.Base.DurationSpecification.DurationPolicy != GameplayEffectDurationPolicy.Infinite);
            foreach (AbstractGameplayEffectShelfContainer container in FinishedEffects)
            {
                container.OnRemove();
                EffectShelf.Remove(container);
                
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
            
            List<GameplayTagScriptableObject> appliedTags = new List<GameplayTagScriptableObject>();
            
            // Collect applied tags
            foreach (AbstractGameplayEffectShelfContainer container in EffectShelf) appliedTags.AddRange(container.Spec.Base.GrantedTags);

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
            return $"{Data}";
        }
    }
}