using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEditorInternal;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public class AbilitySystemComponent : MonoBehaviour
    {

        protected EAbilityActivationPolicy activationPolicy;
        protected List<AbstractImpactWorkerScriptableObject> impactWorkers;
        protected List<AbilityScriptableObject> startingAbilities;
        protected bool allowDuplicateAbilities;
        
        private GASComponentBase System;
        private Dictionary<int, AbilityCacheLayer> AbilityCache = new();

        private class AbilityCacheLayer
        {
            /// <summary>
            /// When enabled, abilities can be activated and passively granted tags are applied
            /// When disabled, abilities cannot be activated and passively granted tags are removed
            /// </summary>
            private bool _enabled = true;
            public bool lastEnabled { get; private set; }

            public bool enabled
            {
                get => _enabled;
                set
                {
                    lastEnabled = _enabled;
                    _enabled = value;
                }
            }
            
            /// <summary>
            /// When locked, abilities cannot be activated but passively granted tags remain applied
            /// When unlocked, abilities can be activated and passively granted tags remain applied
            /// </summary>
            private bool _locked = false;
            public bool lastLocked { get; private set; }

            public bool locked
            {
                get => _locked;
                set
                {
                    lastLocked = _locked;
                    _locked = value;
                }
            }
            
            public bool active => activeContainer != null;
            public AbilitySpecContainer activeContainer;
            
            public Dictionary<int, AbilitySpecContainer> Cache = new();
        }
        
        private ImpactWorkerCache ImpactWorkerCache;

        public bool Executing => activeLayers.Count > 0;
        private HashSet<int> activeLayers = new();
        private void SetActiveContainer(int layer, AbilitySpecContainer container)
        {
            bool status = container is not null ? activeLayers.Add(layer) : activeLayers.Remove(layer);
            if (!status) return;
            AbilityCache[layer].activeContainer = container;
        }
        private AbilitySpecContainer GetActiveContainer(int layer)
        {
            return AbilityCache[layer].activeContainer;
        }
        private bool IsActive(int layer) => AbilityCache.ContainsKey(layer) && AbilityCache[layer].active;
        private bool IsActive(int layer, int index) => IsActive(layer) && AbilityCache[layer].Cache.ContainsKey(index) && AbilityCache[layer].Cache[index] == AbilityCache[layer].activeContainer;
        private bool IsActive(int layer, AbilitySpecContainer container) => IsActive(layer) && container != null && AbilityCache[layer].activeContainer == container;
        private Queue<AbilityActivationRequest> activationQueue = new();

        public struct AbilityActivationRequest
        {
            public int Layer;
            public int Index;

            public AbilityActivationRequest(int layer, int index)
            {
                Layer = layer;
                Index = index;
            }
        }

        private bool _enabled;
        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (value == _enabled) return;
                if (!value) activationQueue.Clear();
                if (Executing && !value) InjectAll(EAbilityInjection.INTERRUPT);
                if (!value)
                {
                    foreach (int layer in AbilityCache.Keys) SetLayerEnabled(layer, false);
                }
                _enabled = value;
            }
        }

        private bool _locked;

        public bool Locked
        {
            get => _locked;
            set
            {
                if (value == _locked) return;
                if (!value)
                {
                    foreach (int layer in AbilityCache.Keys) SetLayerLocked(layer, false);
                }
                _locked = value;
            }
        }
        
        #region Callbacks

        private Action<AbilityDataPacket> _onAbilityCast;
        private event Action<AbilityDataPacket> OnAbilityCast
        {
            add
            {
                if (Array.IndexOf(_onAbilityCast.GetInvocationList(), value) == -1) _onAbilityCast += value;
            }
            remove => _onAbilityCast -= value;
        }
        
        private Action<AbilityDataPacket> _onAbilityTargetStart;
        private event Action<AbilityDataPacket> OnAbilityTargetStart
        {
            add
            {
                if (Array.IndexOf(_onAbilityTargetStart.GetInvocationList(), value) == -1) _onAbilityTargetStart += value;
            }
            remove => _onAbilityTargetStart -= value;
        }
        
        private Action<AbilityDataPacket> _onAbilityTargetEnd;
        private event Action<AbilityDataPacket> OnAbilityTargetEnd
        {
            add
            {
                if (Array.IndexOf(_onAbilityTargetEnd.GetInvocationList(), value) == -1) _onAbilityTargetEnd += value;
            }
            remove => _onAbilityTargetEnd -= value;
        }
        
        private Action<AbilityDataPacket> _onAbilityCastStart;
        private event Action<AbilityDataPacket> OnAbilityCastStart
        {
            add
            {
                if (Array.IndexOf(_onAbilityCastStart.GetInvocationList(), value) == -1) _onAbilityCastStart += value;
            }
            remove => _onAbilityCastStart -= value;
        }
        
        private Action<AbilityDataPacket> _onAbilityCastEnd;
        private event Action<AbilityDataPacket> OnAbilityCastEnd
        {
            add
            {
                if (Array.IndexOf(_onAbilityCastEnd.GetInvocationList(), value) == -1) _onAbilityCastEnd += value;
            }
            remove => _onAbilityCastEnd -= value;
        }
        
        private Action<AbilityDataPacket, AbstractAbilityProxyTaskScriptableObject> _onAbilityTaskActivate;
        private event Action<AbilityDataPacket, AbstractAbilityProxyTaskScriptableObject> OnAbilityTaskActivate
        {
            add
            {
                if (Array.IndexOf(_onAbilityTaskActivate.GetInvocationList(), value) == -1) _onAbilityTaskActivate += value;
            }
            remove => _onAbilityTaskActivate -= value;
        }
        
        private Action<AbilityDataPacket, AbstractAbilityProxyTaskScriptableObject> _onAbilityTaskEnd;
        private event Action<AbilityDataPacket, AbstractAbilityProxyTaskScriptableObject> OnAbilityTaskEnd
        {
            add
            {
                if (Array.IndexOf(_onAbilityTaskEnd.GetInvocationList(), value) == -1) _onAbilityTaskEnd += value;
            }
            remove => _onAbilityTaskEnd -= value;
        }
        
        private Action<AbilityDataPacket> _onAbilityEnd;
        
        #endregion
        
        public virtual void Initialize(GASComponentBase system)
        {
            System = system;
            AbilityCache = new Dictionary<int, AbilityCacheLayer>();
            if (ImpactWorkerCache is null) ImpactWorkerCache = new ImpactWorkerCache(impactWorkers);
            
            foreach (AbilityScriptableObject ability in startingAbilities)
            {
                GiveAbility(ability, ability.StartingLevel, out _);
            }

            Enabled = true;
        }

        public void ProvidePrerequisiteData(ISystemData systemData)
        {
            activationPolicy = systemData.GetActivationPolicy();
            impactWorkers = systemData.GetImpactWorkers();
            startingAbilities = systemData.GetStartingAbilities();
            allowDuplicateAbilities = systemData.GetAllowDuplicateAbilities();
            
            ImpactWorkerCache = new ImpactWorkerCache(impactWorkers);
        }
        
        public void SetAbilitiesLevel(int level)
        {
            foreach (AbilityCacheLayer layer in AbilityCache.Values)
            {
                foreach (var container in layer.Cache.Values)
                {
                    container.Spec.SetLevel(Mathf.Min(level, container.Spec.Base.GetMaxLevel()));
                }
            }
        }
        
        #region Ability Managing
        
        public bool HasAbility(IAbilityData ability)
        {
            return AbilityCache.Keys.Any(layer => HasAbility(ability, layer));
        }

        public bool HasAbility(IAbilityData ability, int layer)
        {
            return AbilityCache.ContainsKey(layer) && AbilityCache[layer].Cache.Values.Any(c => c.Spec.Base == ability);
        }

        private bool TryGetAbilityContainer(IAbilityData ability, out AbilitySpecContainer container)
        {
            foreach (int layer in AbilityCache.Keys)
            {
                if (TryGetAbilityContainer(ability, layer, out container)) return true;
            }

            container = default;
            return false;
        }
        
        private bool TryGetAbilityContainer(IAbilityData ability, int layer, out AbilitySpecContainer container)
        {
            foreach (var _container in AbilityCache[layer].Cache.Values.Where(_container => _container.Spec.Base == ability))
            {
                container = _container;
                return true;
            }

            container = default;
            return false;
        }
        
        public bool GiveAbility(IAbilityData ability, int level, out int abilityIndex, bool initEnabled = true, bool initLocked = false)
        {
            abilityIndex = -1;
            if (!allowDuplicateAbilities && HasAbility(ability, ability.GetDefinition().Layer)) return false;
            
            AbilityCache.TryAdd(ability.GetDefinition().Layer, new AbilityCacheLayer());
            
            abilityIndex = GetFirstAvailableCacheIndex(ability.GetDefinition().Layer);
            if (abilityIndex < 0) return false;
            
            AbilitySpecContainer container = new AbilitySpecContainer(ability.Generate(System, level));
            
            AbilityCache[ability.GetDefinition().Layer].Cache[abilityIndex] = container;

            InitializeNewAbility(abilityIndex, ability, initEnabled, initLocked);
            
            return true;
        }

        public bool RemoveAbility(IAbilityData ability)
        {
            if (IsActive(ability.GetDefinition().Layer))
            {
                if (AbilityCache[ability.GetDefinition().Layer].activeContainer.Spec.Base == ability) Inject(ability.GetDefinition().Layer, EAbilityInjection.INTERRUPT);
            }

            if (!TryGetCacheIndexOf(ability, out int index)) return false;
            AbilityCache[ability.GetDefinition().Layer].Cache.Remove(index);
            if (AbilityCache[ability.GetDefinition().Layer].Cache.Count == 0) AbilityCache.Remove(ability.GetDefinition().Layer);
            return true;
        }
        
        private bool TryGetCacheIndexOf(IAbilityData ability, out int cacheIndex)
        {
            cacheIndex = -1;
            foreach (int index in AbilityCache[ability.GetDefinition().Layer].Cache.Keys.Where(index => AbilityCache[ability.GetDefinition().Layer].Cache[index].Spec.Base == ability))
            {
                cacheIndex = index;
                return true;
            }

            return false;
        }

        private int GetFirstAvailableCacheIndex(int layer)
        {
            Debug.Log(AbilityCache);
            Debug.Log(AbilityCache[layer]);
            Debug.Log(AbilityCache[layer].Cache);
            for (int i = AbilityCache[layer].Cache.Count; i >= 0; i--)
            {
                if (!AbilityCache[layer].Cache.ContainsKey(i)) return i;
            }

            return -1;
        }

        private void InitializeNewAbility(int abilityIndex, IAbilityData ability, bool initEnabled, bool initLocked)
        {
            SetLayerEnabled(ability.GetDefinition().Layer, initEnabled);
            SetLayerLocked(ability.GetDefinition().Layer, initLocked);

            var req = new AbilityActivationRequest(ability.GetDefinition().Layer, abilityIndex);
            switch (ability.GetDefinition().Type)
            {
                case EAbilityType.Activated:
                    if (ability.GetDefinition().ActivateImmediately) TryActivateAbility(req);
                    break;
                case EAbilityType.AlwaysActive:
                    TryActivateAbility(req);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void HandleTags(IEnumerable<ITag> tags, bool flag)
        {
            if (flag) System.TagCache.AddTags(tags);
            else System.TagCache.RemoveTags(tags);
        }

        #endregion

        #region Ability Handling

        public bool CanActivateAbility(int layer, int index)
        {
            return !Locked 
                   && AbilityCache[layer].Cache.TryGetValue(index, out AbilitySpecContainer container)
                   && container.Spec.ValidateActivationRequirements()
                   && (!container.Spec.Base.GetIgnoreWhenLevelZero() || container.Spec.Level > 0);
        }

        public bool TryActivateAbility(AbilityActivationRequest req)
        {
            if (!CanActivateAbility(req.Layer, req.Index)) return false;
            return ProcessActivationRequest(req.Layer, req.Index);
        }

        private bool ProcessActivationRequest(int layer, int abilityIndex)
        {
            if (AbilityCache[layer].Cache[abilityIndex].Spec.Base.GetDefinition().AlwaysValidToActivate)
            {
                return ActivateAbility(AbilityCache[layer].Cache[abilityIndex]);
            }
            
            return activationPolicy switch
            {
                EAbilityActivationPolicy.NoRestrictions => NoRestrictionsTargetingValidation(layer, abilityIndex) && ActivateAbility(AbilityCache[layer].Cache[abilityIndex]),
                EAbilityActivationPolicy.SingleActive => !Executing && ActivateAbility(AbilityCache[layer].Cache[abilityIndex]),
                EAbilityActivationPolicy.SingleActiveQueue => !Executing ? ActivateAbility(AbilityCache[layer].Cache[abilityIndex]) : QueueAbilityActivation(layer, abilityIndex),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        /// <summary>
        /// Ensures that the same targeting proxy task is not active at the same time when using NoRestrictions policy.
        /// </summary>
        /// <param name="layer">Target ability layer</param>
        /// <param name="abilityIndex">Index of the activated ability</param>
        /// <returns></returns>
        private bool NoRestrictionsTargetingValidation(int layer, int abilityIndex)
        {
            if (!Executing) return true;
            var container = GetActiveContainer(layer);
            if (!container.IsTargeting) return true;
            return container.Spec.Base.GetProxy().TargetingProxy != AbilityCache[layer].Cache[abilityIndex].Spec.Base.GetProxy().TargetingProxy;
        }
        
        private bool ActivateAbility(AbilitySpecContainer container)
        {
            container.Spec.ApplyUsageEffects();
            return container.ActivateAbility(AbilityDataPacket.GenerateFrom(container.Spec, container.Spec.Base.GetProxy().UseImplicitTargeting));
        }

        private bool QueueAbilityActivation(int layer, int abilityIndex)
        {
            activationQueue.Enqueue(new AbilityActivationRequest(layer, abilityIndex));

            return true;
        }

        private void ClearAbilityCache()
        {
            if (AbilityCache is null) return;
            
            foreach (int layer in AbilityCache.Keys)
            {
                ClearAbilityCache(layer);
            }

            AbilityCache.Clear();
        }
        
        private void ClearAbilityCache(int layer)
        {
            if (AbilityCache is null) return;
            
            foreach (int index in AbilityCache.Keys)
            {
                AbilityCache[layer].Cache[index].CleanAndRelease();
            }

            AbilityCache[layer].Cache.Clear();
        }

        public void Inject(int layer, EAbilityInjection injection)
        {
            if (!Executing) return;
            GetActiveContainer(layer)?.Inject(injection);
        }

        public void InjectAll(EAbilityInjection injection)
        {
            foreach (int active in activeLayers) GetActiveContainer(active).Inject(injection);
        }

        private void ClaimActive(AbilitySpecContainer container)
        {
            Debug.Log($"[ ABIL-{System.Identity.DistinctName}-CLAIM ] {container} ");
            
            switch (activationPolicy)
            {
                case EAbilityActivationPolicy.NoRestrictions:
                    break;
                case EAbilityActivationPolicy.SingleActive:
                    if (IsActive(container.Spec.Base.GetDefinition().Layer))
                    {
                        if (container.Spec.Base.GetDefinition().AlwaysValidToActivate) return;
                        AbilityCache[container.Spec.Base.GetDefinition().Layer].activeContainer.Inject(EAbilityInjection.INTERRUPT);
                    }
                    SetActiveContainer(container.Spec.Base.GetDefinition().Layer, container);
                    break;
                case EAbilityActivationPolicy.SingleActiveQueue:
                    if (!AbilityCache[container.Spec.Base.GetDefinition().Layer].active) SetActiveContainer(container.Spec.Base.GetDefinition().Layer, container);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ReleaseClaim(AbilitySpecContainer container)
        {
            Debug.Log($"[ ABIL-{System.Identity.DistinctName}-RELEASE ] {container} ");
            if (AbilityCache[container.Spec.Base.GetDefinition().Layer].activeContainer == container)
            {
                SetActiveContainer(container.Spec.Base.GetDefinition().Layer, null);
            }

            if (activationPolicy == EAbilityActivationPolicy.SingleActiveQueue && activationQueue.Count > 0)
            {
                TryActivateAbility(activationQueue.Dequeue());
            }
        }
        
        /// <summary>
        /// When ENABLED, all passively granted tags are applied as well as active
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="flag"></param>
        public void SetLayerEnabled(int layer, bool flag)
        {
            if (!AbilityCache.ContainsKey(layer) || AbilityCache[layer].lastEnabled == flag) return;
            AbilityCache[layer].enabled = flag;
            
            //if (flag) HandleTags();
        }

        public void SetLayerLocked(int layer, bool flag)
        {
            if (!AbilityCache.ContainsKey(layer) || AbilityCache[layer].lastLocked == flag) return;
            AbilityCache[layer].locked = flag;
        }
        
        #endregion
        
        #region Impact Workers

        public void ProvideFrameImpact(AbilityImpactData impactData)
        {
            impactData.SourcedModifier.BaseDerivation.TrackImpact(impactData);
            impactData.SourcedModifier.BaseDerivation.RunEffectImpactWorkers(impactData);
            ImpactWorkerCache.RunImpactData(impactData);
        }
        
        #endregion
        
        #region Native
        
        private void OnDestroy()
        {
            ClearAbilityCache();
        }
        
        #endregion
        
        private class AbilitySpecContainer
        {
            public AbilitySpec Spec;
            
            public bool IsActive { get; private set; }
            public bool IsTargeting { get; private set; }
            public bool IsClaiming => IsTargeting || IsActive;
            
            private AbilityProxy Proxy;
            private CancellationTokenSource cts;
            private CancellationTokenSource targetingCts;
            
            public AbilitySpecContainer(AbilitySpec spec)
            {
                Spec = spec;
                IsActive = false;

                Proxy = Spec.Base.GetProxy().GenerateProxy();
                ResetTokens();
            }
            
            public bool ActivateAbility(AbilityDataPacket implicitData)
            {
                if (IsClaiming) return false;  // Prevent reactivation mid-use
                
                Spec.Owner.AbilitySystem.ClaimActive(this);
                implicitData.AddPayload(ITag.Get(TagChannels.PAYLOAD_DERIVATION), Spec);

                Reset();
                
                AwaitAbility(implicitData).Forget();

                return true;
            }

            private void Reset()
            {
                IsActive = false;
                IsTargeting = false;

                ResetTokens();
            }

            private async UniTaskVoid AwaitAbility(AbilityDataPacket data)
            {
                bool targetingCancelled = false;
                try
                {
                    IsTargeting = true;
                    await Proxy.ActivateTargetingTask(targetingCts.Token, data);
                }
                catch (OperationCanceledException)
                {
                    // Targeting is cancelled
                    targetingCancelled = true;
                }
                finally
                {
                    IsTargeting = false;
                    
                    if (data.TryGet(ITag.Get(TagChannels.PAYLOAD_GAS), EProxyDataValueTarget.Primary, out GASComponentBase target) && !Spec.ValidateActivationRequirements(target))
                    {
                        // Do invalid target feedback here
                        
                        targetingCancelled = true;
                    }
                }

                if (targetingCancelled)
                {
                    CleanAndRelease();
                    return;
                }

                try
                {
                    IsActive = true;
                        
                    Spec.Owner.TagCache.AddTags(Spec.Base.GetTags().ActiveGrantedTags);

                    await Proxy.Activate(cts.Token, data);
                }
                catch (OperationCanceledException)
                {
                    // Ability in execution is interrupted (cancelled)
                    Debug.Log($"Cancelled!");
                }
                finally
                {
                    IsActive = false;
                    Spec.Owner.TagCache.RemoveTags(Spec.Base.GetTags().ActiveGrantedTags);
                }
                
                CleanAndRelease();
            }

            public void Inject(EAbilityInjection injection)
            {
                if (!IsClaiming) return;

                Proxy.Inject(injection);

                if (injection == EAbilityInjection.INTERRUPT)
                {
                    cts?.Cancel();
                }
            }
            
            public void CleanAndRelease()
            {
                Proxy.Clean();
                
                CleanTargetingToken();
                CleanActivationToken();
                
                Spec.Owner.AbilitySystem.ReleaseClaim(this);
            }

            private void CleanTargetingToken()
            {
                if (targetingCts is null) return;
                
                if (!targetingCts.IsCancellationRequested) targetingCts?.Cancel();
                targetingCts?.Dispose();
                targetingCts = null;
            }

            private void CleanActivationToken()
            {
                if (cts is null) return;
                
                if (!cts.IsCancellationRequested) cts?.Cancel();
                cts?.Dispose();
                cts = null;
            }

            private void ResetTokens()
            {
                CleanTargetingToken();
                CleanActivationToken();
                
                cts = new CancellationTokenSource();
                targetingCts = new CancellationTokenSource();
            }

            public override string ToString()
            {
                return $"{Spec}";
            }
        }
    }

    public enum EAbilityActivationPolicy
    {
        NoRestrictions,  // Always able to activate any available ability
        SingleActive,  // Only able to activate one ability at a time
        SingleActiveQueue  // Only able to activate one ability at a time, but subsequent activations are queued (queue is cleared in the same moment that targeting tasks are cancelled)
    }
}
