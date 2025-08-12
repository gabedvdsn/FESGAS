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
        public EAbilityActivationPolicy DefaultActivationPolicy => activationPolicy;

        protected List<AbstractImpactWorkerScriptableObject> impactWorkers;
        protected List<AbilityScriptableObject> startingAbilities;
        protected bool allowDuplicateAbilities;

        private GASComponentBase Root;

        private Dictionary<int, AbilitySpecContainer> AbilityCache = new();

        private Dictionary<EAbilityActivationPolicy, HashSet<int>> ActiveCache = new()
        {
            { EAbilityActivationPolicy.NoRestrictions, new() },
            { EAbilityActivationPolicy.SingleActive, new() },
            { EAbilityActivationPolicy.SingleActiveQueue, new() }
        };

        private ImpactWorkerCache ImpactWorkerCache;

        public bool IsExecuting() => ActiveCache.Keys.Any(IsExecuting);
        public bool IsExecuting(EAbilityActivationPolicy policy) => ActiveCache[policy].Count > 0;
        
        public bool IsExecutingCritical()
        {
            return IsExecuting() && ActiveCache.Keys.Any(IsExecutingCritical);
        }
        public bool IsExecutingCritical(EAbilityActivationPolicy policy)
        {
            return IsExecuting(policy) && ActiveCache[policy].Any(IsCritical);
        }

        public bool IsCritical(int index) => AbilityCache[index].Spec.Base.GetProxy().Stages.Any(stage => stage.Tasks.Any(task => task.IsCriticalSection));
        
        private Queue<AbilityActivationRequest> activationQueue = new();

        public struct AbilityActivationRequest
        {
            public EAbilityActivationPolicy Policy;
            public int Index;

            public AbilityActivationRequest(EAbilityActivationPolicy policy, int index)
            {
                Policy = policy;
                Index = index;
            }

            public AbilityActivationRequest(IAbilityData ability, int index, AbilitySystemComponent asc = null)
            {
                Policy = ability.GetDefinition().ActivationPolicy.Translate(asc);
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
                if (!value)
                {
                    activationQueue.Clear();
                    InjectAll(EAbilityInjection.INTERRUPT);
                }

                _enabled = value;
            }
        }

        private bool _locked;

        public bool Locked
        {
            get => _locked;
            set => _locked = value;
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
            Root = system;
            AbilityCache = new Dictionary<int, AbilitySpecContainer>();
            ActiveCache = new Dictionary<EAbilityActivationPolicy, HashSet<int>>();
            
            if (ImpactWorkerCache is null) ImpactWorkerCache = new ImpactWorkerCache(impactWorkers);

            foreach (AbilityScriptableObject ability in startingAbilities)
            {
                GiveAbility(ability, ability.StartingLevel, out _);
            }

            Enabled = true;
            Locked = false;
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
            foreach (var container in AbilityCache.Values)
            {
                container.Spec.SetLevel(Mathf.Min(level, container.Spec.Base.GetMaxLevel()));
            }
        }

        #region Ability Managing
        
        public bool HasAbility(IAbilityData ability)
        {
            return AbilityCache.Values.Any(c => c.Spec.Base == ability);
        }

        private bool TryGetAbilityContainer(IAbilityData ability, out AbilitySpecContainer container)
        {
            foreach (var _container in AbilityCache.Values.Where(_container => _container.Spec.Base == ability))
            {
                container = _container;
                return true;
            }

            container = default;
            return false;
        }

        public bool GiveAbility(IAbilityData ability, int level, out int abilityIndex)
        {
            abilityIndex = -1;
            
            if (!Enabled) return false;
            if (!allowDuplicateAbilities && HasAbility(ability)) return false;
            
            abilityIndex = GetFirstAvailableCacheIndex();
            if (abilityIndex < 0) return false;

            AbilitySpecContainer container = new AbilitySpecContainer(ability.Generate(Root, level));
            AbilityCache[abilityIndex] = container;

            HandleTags(ability.GetTags().PassivelyGrantedTags, true);
            InitializeNewAbility(abilityIndex, ability);

            return true;
        }

        public bool RemoveAbility(IAbilityData ability)
        {
            if (!TryGetCacheIndexOf(ability, out int index)) return false;
            
            if (AbilityCache[index].IsClaiming) Inject(index, EAbilityInjection.INTERRUPT);
            
            HandleTags(ability.GetTags().PassivelyGrantedTags, false);

            return AbilityCache.Remove(index);
        }

        private bool TryGetCacheIndexOf(IAbilityData ability, out int cacheIndex)
        {
            cacheIndex = -1;
            foreach (int index in AbilityCache.Keys.Where(index => AbilityCache[index].Spec.Base == ability))
            {
                cacheIndex = index;
                return true;
            }

            return false;
        }

        private int GetFirstAvailableCacheIndex()
        {
            for (int i = AbilityCache.Count; i >= 0; i--)
            {
                if (!AbilityCache.ContainsKey(i)) return i;
            }

            return -1;
        }

        private void InitializeNewAbility(int abilityIndex, IAbilityData ability)
        {
            if (!ability.GetDefinition().ActivateImmediately) return;
            
            var req = new AbilityActivationRequest(ability, abilityIndex, this);
            TryActivateAbility(req);
        }

        private void HandleTags(IEnumerable<ITag> tags, bool flag)
        {
            if (flag) Root.TagCache.AddTags(tags);
            else Root.TagCache.RemoveTags(tags);
        }

        #endregion

        #region Ability Handling

        public bool CanActivateAbility(int index)
        {
            return Enabled 
                   && !Locked
                   && AbilityCache.TryGetValue(index, out AbilitySpecContainer container)
                   && container.Spec.ValidateActivationRequirements()
                   && (!container.Spec.Base.GetIgnoreWhenLevelZero() || container.Spec.Level > 0);
        }

        public bool TryActivateAbility(AbilityActivationRequest req)
        {
            if (!CanActivateAbility(req.Index)) return false;
            return ProcessActivationRequest(req.Index);
        }
        
        private bool ProcessActivationRequest(int abilityIndex)
        {
            var policy = AbilityCache[abilityIndex].Spec.Base.GetDefinition().ActivationPolicy.Translate(this);
            
            return policy switch
            {
                EAbilityActivationPolicy.NoRestrictions => NoRestrictionsTargetingValidation(abilityIndex) && ActivateAbility(AbilityCache[abilityIndex]),
                EAbilityActivationPolicy.SingleActive => !IsExecutingCritical(EAbilityActivationPolicy.SingleActive) && ActivateAbility(AbilityCache[abilityIndex]),
                EAbilityActivationPolicy.SingleActiveQueue => !IsExecutingCritical(EAbilityActivationPolicy.SingleActiveQueue) ? ActivateAbility(AbilityCache[abilityIndex]) : QueueAbilityActivation(abilityIndex),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
        /// <summary>
        /// Ensures that the same targeting proxy task is not active at the same time when using NoRestrictions policy.
        /// </summary>
        /// <param name="abilityIndex">Index of the activated ability</param>
        /// <returns></returns>
        private bool NoRestrictionsTargetingValidation(int abilityIndex)
        {
            if (!IsExecutingCritical(EAbilityActivationPolicy.NoRestrictions)) return true;
            return !IsCritical(abilityIndex);
        }

        private bool ActivateAbility(AbilitySpecContainer container)
        {
            container.Spec.ApplyUsageEffects();
            return container.ActivateAbility(AbilityDataPacket.GenerateFrom(container.Spec, container.Spec.Base.GetProxy().UseImplicitTargeting));
        }

        private bool QueueAbilityActivation(int abilityIndex)
        {
            activationQueue.Enqueue(new AbilityActivationRequest(AbilityCache[abilityIndex].Spec.Base, abilityIndex));

            return true;
        }

        private void ClearAbilityCache()
        {
            if (AbilityCache is null) return;

            foreach (var policy in ActiveCache.Keys)
            {
                foreach (int index in ActiveCache[policy]) AbilityCache[index].Inject(EAbilityInjection.INTERRUPT);
                ActiveCache[policy].Clear();
            }

            AbilityCache.Clear();
        }

        public void Inject(int index, EAbilityInjection injection)
        {
            if (!AbilityCache.TryGetValue(index, out var container) || !container.IsClaiming) return;
            container.Inject(injection);
        }
        
        public void Inject(IAbilityData ability, EAbilityInjection injection)
        {
            if (!TryGetAbilityContainer(ability, out var container)) return;
            if (!container.IsClaiming) return;
            
            container.Inject(injection);
        }
        
        public void Inject(EAbilityActivationPolicy policy, EAbilityInjection injection)
        {
            foreach (int index in ActiveCache[policy])
            {
                if (!AbilityCache[index].IsClaiming) ReleaseClaim(AbilityCache[index]);
                AbilityCache[index].Inject(injection);
            }
        }

        public void InjectAll(EAbilityInjection injection)
        {
            foreach (var policy in ActiveCache.Keys)
            {
                Inject(policy, injection);
            }
        }

        /// <summary>
        /// The ability container claims runtime over the ASC
        /// </summary>
        /// <param name="container">The claiming container</param>
        /// <returns>Whether or not the ASC was successfully claimed</returns>
        private bool ClaimActive(AbilitySpecContainer container)
        {
            Debug.Log($"[ ABIL-{Root.Identity.DistinctName}-CLAIM ] {container} ");

            if (!TryGetCacheIndexOf(container.Spec.Base, out var index)) return false;
            
            ActiveCache[AbilityCache[index].Spec.Base.GetDefinition().ActivationPolicy.Translate(this)].Add(index);
            
            return true;
        }

        private void ReleaseClaim(AbilitySpecContainer container)
        {
            Debug.Log($"[ ABIL-{Root.Identity.DistinctName}-RELEASE ] {container} ");
            
            if (!TryGetCacheIndexOf(container.Spec.Base, out var index)) return;

            var policy = AbilityCache[index].Spec.Base.GetDefinition().ActivationPolicy.Translate(this);
            ActiveCache[policy].Remove(index);

            if (policy == EAbilityActivationPolicy.SingleActiveQueue && activationQueue.Count > 0) TryActivateAbility(activationQueue.Dequeue());
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
                if (IsClaiming) return false; // Prevent reactivation mid-use
                if (!Spec.Owner.AsData().AbilitySystem.ClaimActive(this)) return false;
                
                implicitData.AddPayload(Tags.PAYLOAD_DERIVATION, Spec);

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

                    if (data.TryGet(Tags.PAYLOAD_GAS, EProxyDataValueTarget.Primary, out GASComponentBase target) && !Spec.ValidateActivationRequirements(target))
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

                    Spec.Owner.GetTagCache().AddTags(Spec.Base.GetTags().ActiveGrantedTags);

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
                    Spec.Owner.GetTagCache().RemoveTags(Spec.Base.GetTags().ActiveGrantedTags);
                    
                    CleanAndRelease();
                }
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

                Spec.Owner.AsData().AbilitySystem.ReleaseClaim(this);
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

        private abstract class AbstractAbilityCacheLayer
        {
            public bool locked;

            public abstract AbilitySpecContainer[] GetActiveContainer();
            public abstract void SetActiveContainer(AbilitySpecContainer container);
        }

        private class SingleActiveAbilityCacheLayer : AbstractAbilityCacheLayer
        {
            private AbilitySpecContainer activeContainer;

            public override AbilitySpecContainer[] GetActiveContainer()
            {
                return new[] { activeContainer };
            }
            public override void SetActiveContainer(AbilitySpecContainer container)
            {
                activeContainer = container;
            }
        }

        private class ManyActiveAbilityCacheLayer : AbstractAbilityCacheLayer
        {
            private List<AbilitySpecContainer> activeContainers = new();
            
            public override AbilitySpecContainer[] GetActiveContainer()
            {
                return activeContainers.ToArray();
            }
            public override void SetActiveContainer(AbilitySpecContainer container)
            {
                activeContainers.Add(container);
            }
        }
    }

    public enum EAbilityActivationPolicy
    {
        NoRestrictions,  // Always able to activate any available ability
        SingleActive,  // Only able to activate one ability at a time
        SingleActiveQueue  // Only able to activate one ability at a time, but subsequent activations are queued (queue is cleared in the same moment that targeting tasks are cancelled)
    }

    public enum EAbilityActivationPolicyExtended
    {
        UseLocal,
        NoRestrictions,
        SingleActive,
        SingleActiveQueue
    }
}
