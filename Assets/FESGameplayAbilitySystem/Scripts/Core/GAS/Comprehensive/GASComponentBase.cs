using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace FESGameplayAbilitySystem
{
    public abstract class GASComponentBase : AbstractMonoProcess, IGameplayProcessHandler
    {
        [Header("Gameplay Ability System")]
        
        public GASIdentityData Identity;
        
        // Subsystems
        [HideInInspector] public AttributeSystemComponent AttributeSystem;
        [HideInInspector] public AbilitySystemComponent AbilitySystem;
        
        // Core
        private List<AbstractGameplayEffectShelfContainer> EffectShelf;
        private List<AbstractGameplayEffectShelfContainer> FinishedEffects;
        private bool needsCleaning;
        private bool isActive;
        
        // Tags
        public TagCache TagCache;
        
        // Process
        private Dictionary<int, ProcessRelay> Relays;
        
        protected void Awake()
        {
            AttributeSystem = GetComponent<AttributeSystemComponent>();
            AbilitySystem = GetComponent<AbilitySystemComponent>();
            
            EffectShelf = new List<AbstractGameplayEffectShelfContainer>();
            FinishedEffects = new List<AbstractGameplayEffectShelfContainer>();

            Relays = new Dictionary<int, ProcessRelay>();
            
            Identity.Initialize(this);
            
            PrepareSystem();
            
            AttributeSystem.Initialize(this);
            AbilitySystem.Initialize(this);
        }

        protected abstract void PrepareSystem();
        
        private void Update()
        {
            TickEffectShelf();
            
            if (needsCleaning) ClearFinishedEffects();
            
            TagCache.TickTagWorkers();
        }
        
        #region Process Parameters
        
        // Handling
        public bool HandlerValidateAgainst(IGameplayProcessHandler handler)
        {
            return (GASComponentBase)handler == this;
        }

        public void HandlerSubscribeProcess(ProcessRelay relay)
        {
            Relays[relay.CacheIndex] = relay;
        }

        public bool HandlerVoidProcess(int processIndex)
        {
            return Relays.Remove(processIndex);
        }
        
        // Process
        public override void WhenInitialize(ProcessRelay relay)
        {
            isActive = true;
        }
        public override void WhenUpdate(ProcessRelay relay)
        {
            TickEffectShelf();
            
            if (needsCleaning) ClearFinishedEffects();
            
            TagCache.TickTagWorkers();
        }
        public override void WhenWait(ProcessRelay relay)
        {
            //
        }
        public override void WhenTerminate(ProcessRelay relay)
        {
            //
        }
        public override async UniTask RunProcess(ProcessRelay relay, CancellationToken token)
        {
            await UniTask.WaitWhile(() => isActive, cancellationToken: token);
        }
        
        #endregion
        
        #region Effect Handling
        
        public GameplayEffectSpec GenerateEffectSpec(IEffectDerivation derivation, IEffectBase GameplayEffect)
        {
            return GameplayEffect.Generate(derivation, this);
        }

        public bool ApplyGameplayEffect(GameplayEffectSpec spec)
        {
            if (spec is null) return false;
            
            if (!ValidateEffectApplicationRequirements(spec)) return false;
            
            switch (spec.Base.GetDurationPolicy())
            {
                case EEffectDurationPolicy.Instant:
                    ApplyInstantGameplayEffect(spec);
                    break;
                case EEffectDurationPolicy.Infinite:
                case EEffectDurationPolicy.Durational:
                    ApplyDurationalGameplayEffect(spec);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            foreach (var containedEffect in spec.Base.GetContainedEffects(EApplyDuringRemove.OnApply))
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
        public bool ApplyGameplayEffect(IEffectDerivation derivation, IEffectBase GameplayEffect)
        {
            GameplayEffectSpec spec = GenerateEffectSpec(derivation, GameplayEffect);
            return ApplyGameplayEffect(spec);
        }

        public void RemoveGameplayEffect(IEffectBase effect)
        {
            RemoveGameplayEffect(effect.GetIdentifier());
        }

        public void RemoveGameplayEffect(GameplayTagScriptableObject identifier)
        {
            List<AbstractGameplayEffectShelfContainer> toRemove = EffectShelf.Where(container => container.Spec.Base.GetIdentifier() == identifier).ToList();
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
            if (!AttributeSystem.DefinesAttribute(spec.Base.GetAttributeTarget())) return;

            if (TryHandleExistingDurationalGameplayEffect(spec)) return;

            AbstractGameplayEffectShelfContainer container;
            switch (spec.Base.GetReApplicationPolicy())
            {
                case EEffectReApplicationPolicy.Refresh:
                case EEffectReApplicationPolicy.Extend:
                case EEffectReApplicationPolicy.Append:
                    container = GameplayEffectShelfContainer.Generate(spec, ValidateEffectOngoingRequirements(spec));
                    break;
                case EEffectReApplicationPolicy.Stack:
                case EEffectReApplicationPolicy.StackRefresh:
                case EEffectReApplicationPolicy.StackExtend:
                    container = StackableGameplayShelfContainer.Generate(spec, ValidateEffectOngoingRequirements(spec));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
                
            EffectDebugManager.Instance.CreateDebugFor(ref container);
            TagsDebugManager.Instance.CreateDebugFor(ref container);
            
            EffectShelf.Add(container);
            TagCache.AddTags(container.Spec.Base.GetGrantedTags());
            
            container.RunEffectApplicationWorkers();
                
            if (spec.Base.GetTickOnApplication())
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
            if (!AttributeSystem.TryGetAttributeValue(spec.Base.GetAttributeTarget(), out AttributeValue attributeValue)) return;

            // Apply tags
            TagCache.AddTags(spec.Base.GetGrantedTags());
            
            SourcedModifiedAttributeValue sourcedModifiedValue = spec.SourcedImpact(attributeValue);
            
            AttributeSystem.ModifyAttribute(spec.Base.GetAttributeTarget(), sourcedModifiedValue);
            
            spec.RunEffectApplicationWorkers();
            spec.RunEffectRemovalWorkers();
            
            HandleGameplayEffects();
            TagCache.RemoveTags(spec.Base.GetGrantedTags());
        }
        
        /// <summary>
        /// Instantly applies the effects of a durational/infinite gameplay effect
        /// </summary>
        /// <param name="container"></param>
        private void ApplyInstantGameplayEffect(AbstractGameplayEffectShelfContainer container)
        {
            if (!AttributeSystem.TryGetAttributeValue(container.Spec.Base.GetAttributeTarget(), out AttributeValue attributeValue)) return;
            
            SourcedModifiedAttributeValue sourcedModifiedValue = container.Spec.SourcedImpact(container, attributeValue);
            
            AttributeSystem.ModifyAttribute(container.Spec.Base.GetAttributeTarget(), sourcedModifiedValue);
            
            container.RunEffectApplicationWorkers();
            
            foreach (var containedEffect in container.Spec.Base.GetContainedEffects(EApplyDuringRemove.OnTick))
            {
                ApplyGameplayEffect(container.Spec.Derivation, containedEffect);
            }
        }
        
        private bool TryHandleExistingDurationalGameplayEffect(GameplayEffectSpec spec)
        {
            if (!TryGetEffectContainer(spec.Base, out AbstractGameplayEffectShelfContainer container)) return false;
            
            switch (spec.Base.GetReApplicationPolicy())
            {
                case EEffectReApplicationPolicy.Refresh:
                    container.Refresh();
                    return true;
                case EEffectReApplicationPolicy.Extend:
                    container.Extend(spec.Base.GetTotalDuration(spec));
                    return true;
                case EEffectReApplicationPolicy.Append:
                    return false;
                case EEffectReApplicationPolicy.Stack:
                    container.Stack();
                    return true;
                case EEffectReApplicationPolicy.StackRefresh:
                    container.Refresh();
                    return true;
                case EEffectReApplicationPolicy.StackExtend:
                    container.Extend(spec.Base.GetTotalDuration(spec));
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

        private void TickEffectShelf()
        {
            foreach (AbstractGameplayEffectShelfContainer container in EffectShelf)
            {
                EEffectDurationPolicy durationPolicy = container.Spec.Base.GetDurationPolicy();
                
                if (durationPolicy == EEffectDurationPolicy.Instant) continue;
                
                float deltaTime = Time.deltaTime;
                
                container.RunEffectTickWorkers();
                container.TickPeriodic(deltaTime, out int executeTicks);
                if (executeTicks > 0 && container.Ongoing)
                {
                    for (int _ = 0; _ < executeTicks; _++)
                    {
                        ApplyInstantGameplayEffect(container);
                    }
                }

                if (durationPolicy == EEffectDurationPolicy.Infinite) continue;
                
                container.UpdateTimeRemaining(deltaTime);

                if (container.DurationRemaining > 0) continue;
                
                FinishedEffects.Add(container);
                needsCleaning = true;
            }
        }
        
        private void ClearFinishedEffects()
        {
            //EffectShelf.RemoveAll(container => container.DurationRemaining <= 0 && container.Spec.Base.DurationSpecification.DurationPolicy != EEffectDurationPolicy.Infinite);
            foreach (AbstractGameplayEffectShelfContainer container in FinishedEffects)
            {
                container.OnRemove();
                EffectShelf.Remove(container);
                container.RunEffectRemovalWorkers();
                
                TagCache.RemoveTags(container.Spec.Base.GetGrantedTags());
                
                if (container.RetainAttributeImpact()) AttributeSystem.RemoveAttributeDerivation(container);
            }
            
            FinishedEffects.Clear();
            needsCleaning = false;
            
            HandleGameplayEffects();
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
            return spec.Base.ValidateApplicationRequirements(spec);
            
            /*List<GameplayTagScriptableObject> appliedTags = GetAppliedTags();

            // Validate target (this) application requirements
            return spec.Base.TargetRequirements.CheckApplicationRequirements(appliedTags) &&
                   !spec.Base.TargetRequirements.CheckRemovalRequirements(appliedTags) &&
                   // Validate source application requirements
                   spec.Source.ValidateEffectApplicationRequirements(spec.Base.SourceRequirements) &&
                   !spec.Source.ValidateEffectRemovalRequirements(spec.Base.SourceRequirements);*/
        }
        
        /// <summary>
        /// Are the application requirements met? (i.e. should the effect be applied?)
        /// </summary>
        /// <param name="requirements"></param>
        /// <returns></returns>
        private bool ValidateEffectApplicationRequirements(GameplayEffectRequirements requirements)
        {
            return true;
            // return requirements.CheckApplicationRequirements(GetAppliedTags());
        }
        
        /// <summary>
        /// Should the spec be ongoing?
        /// </summary>
        /// <param name="spec"></param>
        /// <returns></returns>
        private bool ValidateEffectOngoingRequirements(GameplayEffectSpec spec)
        {
            return spec.Base.ValidateOngoingRequirements(spec);
            
            /*// Validate source application requirements
            return spec.Base.TargetRequirements.CheckOngoingRequirements(GetAppliedTags()) &&
                   // Validate target application requirements
                   spec.Source.ValidateEffectOngoingRequirements(spec.Base.SourceRequirements);*/
        }
        
        /// <summary>
        /// Are the ongoing requirements met? (i.e. should the effect remain ongoing?)
        /// </summary>
        /// <param name="requirements"></param>
        /// <returns></returns>
        private bool ValidateEffectOngoingRequirements(GameplayEffectRequirements requirements)
        {
            return true;
            // return requirements.CheckOngoingRequirements(GetAppliedTags());
        }
        
        /// <summary>
        /// Should the spec be removed?
        /// </summary>
        /// <param name="spec"></param>
        /// <returns></returns>
        private bool ValidateEffectRemovalRequirements(GameplayEffectSpec spec)
        {
            return spec.Base.ValidateRemovalRequirements(spec);
            
            /*// Validate source application requirements
            return spec.Base.TargetRequirements.CheckRemovalRequirements(GetAppliedTags()) ||
                   // Validate target application requirements
                   spec.Source.ValidateEffectRemovalRequirements(spec.Base.SourceRequirements);*/
        }

        /// <summary>
        /// Are the removal requirements met? (i.e. should the effect be removed?)
        /// </summary>
        /// <param name="requirements"></param>
        /// <returns></returns>
        private bool ValidateEffectRemovalRequirements(GameplayEffectRequirements requirements)
        {
            return true;
            // return requirements.CheckRemovalRequirements(TagCache.GetAppliedTags());
        }
        
        #endregion
        
        #region Effect Helpers

        public bool TryGetEffectContainer(IEffectBase effectBase, out AbstractGameplayEffectShelfContainer container)
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
                foreach (GameplayTagScriptableObject specTag in container.Spec.Base.GetGrantedTags())
                {
                    if (!lookForTags.Contains(specTag)) continue;
                    if (container.Spec.Base.GetDurationPolicy() == EEffectDurationPolicy.Infinite)
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
