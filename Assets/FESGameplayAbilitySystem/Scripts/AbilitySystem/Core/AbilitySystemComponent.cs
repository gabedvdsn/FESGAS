using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public class AbilitySystemComponent : MonoBehaviour
    {
        protected EAbilityActivationPolicy activationPolicy;
        protected int maxAbilities;
        protected List<AbstractApplicationWorkerScriptableObject> applicationWorkers;
        protected List<AbstractImpactWorkerScriptableObject> impactWorkers;
        protected List<AbilityScriptableObject> startingAbilities;
        
        private GASComponentBase System;
        private Dictionary<int, AbilitySpecContainer> AbilityCache = new();
        private List<AbilityImpactData> FrameImpactData = new();

        public bool Executing => abilityActive && activeContainer is not null;
        private bool abilityActive;
        private AbilitySpecContainer activeContainer = null;
        private Queue<int> activationQueue = new();
        
        public virtual void Initialize(GASComponentBase system)
        {
            System = system;
            AbilityCache = new Dictionary<int, AbilitySpecContainer>();
            FrameImpactData = new List<AbilityImpactData>();
            
            foreach (AbilityScriptableObject ability in startingAbilities)
            {
                GiveAbility(ability, 1, out _);
            }
        }

        public void ProvidePrerequisiteData(GASSystemData systemData)
        {
            activationPolicy = systemData.ActivationPolicy;
            maxAbilities = systemData.MaxAbilities;
            applicationWorkers = systemData.ApplicationWorkers;
            impactWorkers = systemData.ImpactWorkers;
            startingAbilities = systemData.StartingAbilities;
        }
        
        public void SetAbilitiesLevel(int level)
        {
            foreach (AbilitySpecContainer container in AbilityCache.Values)
            {
                container.Spec.SetLevel(Mathf.Min(level, container.Spec.Base.MaxLevel));
            }
        }

        public int GetMaxAbilityLevel()
        {
            return AbilityCache.Values.Max(container => container.Spec.Base.MaxLevel);
        }
        
        #region Ability Managing

        public bool CanGiveAbility(AbilityScriptableObject ability) => HasRoomInCache() && !HasAbility(ability);
        
        public bool HasRoomInCache() => AbilityCache.Count < maxAbilities;
        
        public bool HasAbility(AbilityScriptableObject ability)
        {
            return AbilityCache.Values.Any(c => c.Spec.Base == ability);
        }

        private bool TryGetAbilityContainer(AbilityScriptableObject ability, out AbilitySpecContainer container)
        {
            foreach (AbilitySpecContainer _container in AbilityCache.Values.Where(_container => _container.Spec.Base == ability))
            {
                container = _container;
                Debug.Log($"Successfully found container");
                return true;
            }

            Debug.Log($"Did not find container");
            container = null;
            return false;
        }
        
        public bool GiveAbility(AbilityScriptableObject ability, int Level, out int abilityIndex)
        {
            abilityIndex = -1;
            if (!CanGiveAbility(ability)) return false;
            
            int index = GetFirstAvailableCacheIndex();
            if (index < 0) return false;
            abilityIndex = index;
            
            AbilitySpecContainer container = new AbilitySpecContainer(ability.Generate(System, Level));
            AbilityCache[index] = container;

            InitializeNewAbility(index, ability);
            
            return true;
        }

        public bool RevokeAbility(AbilityScriptableObject ability)
        {
            return TryGetCacheIndexOf(ability, out int index) && RevokeAbility(index);
        }

        public bool RevokeAbility(int index)
        {
            if (!AbilityCache.ContainsKey(index)) return false; 
            
            AbilityCache[index].ReleaseAndClean();
            System.RemoveTags(AbilityCache[index].Spec.Base.Tags.PassivelyGrantedTags);

            return AbilityCache.Remove(index);
        }

        public bool SwapAbilityIndices(AbilityScriptableObject ability1, AbilityScriptableObject ability2)
        {
            if (!(TryGetCacheIndexOf(ability1, out int firstIndex) && TryGetCacheIndexOf(ability2, out int secondIndex))) return false;

            return SwapAbilities(firstIndex, secondIndex);
            /*(AbilityCache[firstIndex], AbilityCache[secondIndex]) = (AbilityCache[secondIndex], AbilityCache[firstIndex]);
            return true;*/
        }

        public bool SwapAbilities(int index1, int index2)
        {
            if (AbilityCache.ContainsKey(index1))
            {
                if (AbilityCache.ContainsKey(index1)) (AbilityCache[index1], AbilityCache[index2]) = (AbilityCache[index2], AbilityCache[index1]);
                else if (index2 < maxAbilities)
                {
                    AbilityCache[index2] = AbilityCache[index1];
                    AbilityCache.Remove(index1);
                }

                return true;
            }
            if (AbilityCache.ContainsKey(index2))
            {
                if (index1 < maxAbilities)
                {
                    AbilityCache[index1] = AbilityCache[index2];
                    AbilityCache.Remove(index2);
                }

                return true;
            }

            return false;
        }

        public bool ReplaceAbilityAtIndex(int replaceIndex, AbilityScriptableObject ability, int level)
        {
            if (HasAbility(ability)) return false;
            if (AbilityCache.ContainsKey(replaceIndex)) RevokeAbility(replaceIndex);
            
            AbilitySpecContainer container = new AbilitySpecContainer(ability.Generate(System, level));
            AbilityCache[replaceIndex] = container;

            InitializeNewAbility(replaceIndex, ability);

            return true;
        }

        private bool TryGetCacheIndexOf(AbilityScriptableObject ability, out int cacheIndex)
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
            if (!HasRoomInCache()) return -1;
            for (int i = 0; i <= AbilityCache.Count; i++)
            {
                if (!AbilityCache.ContainsKey(i)) return i;
            }

            return -1;
        }

        private void InitializeNewAbility(int abilityIndex, AbilityScriptableObject ability)
        {
            System.AddTags(ability.Tags.PassivelyGrantedTags, true);

            switch (ability.Definition.Type)
            {
                case EAbilityType.Activated:
                    if (ability.Definition.ActivateImmediately) TryActivateAbility(abilityIndex);
                    break;
                case EAbilityType.AlwaysActive:
                    TryActivateAbility(abilityIndex);
                    break;
                case EAbilityType.Toggled:
                    if (ability.Definition.ActivateImmediately) TryActivateAbility(abilityIndex);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion

        #region Ability Handling

        public bool CanActivateAbility(int index)
        {
            return AbilityCache.TryGetValue(index, out AbilitySpecContainer container) && container.Spec.ValidateActivationRequirements();
        }

        public bool TryActivateAbility(int abilityIndex)
        {
            if (!CanActivateAbility(abilityIndex)) return false;
            return ProcessActivationRequest(abilityIndex);
        }

        private bool ProcessActivationRequest(int abilityIndex)
        {
            return activationPolicy switch
            {
                EAbilityActivationPolicy.NoRestrictions => ActivateAbility(AbilityCache[abilityIndex]),
                EAbilityActivationPolicy.SingleActive => !Executing && ActivateAbility(AbilityCache[abilityIndex]),
                EAbilityActivationPolicy.QueueSingleActive => Executing ? ActivateAbility(AbilityCache[abilityIndex]) : QueueAbilityActivation(abilityIndex),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private bool ActivateAbility(AbilitySpecContainer container)
        {
            container.Spec.ApplyUsageEffects();
            return container.Spec.Base.Proxy.UseImplicitInstructions 
                ? container.ActivateAbility(ProxyDataPacket.GenerateFrom(container.Spec, System, container.Spec.Base.Proxy.OwnerAs)) 
                : container.ActivateAbility(null);
        }

        private bool QueueAbilityActivation(int abilityIndex)
        {
            activationQueue.Enqueue(abilityIndex);

            return true;
        }

        private void ClearAbilityCache()
        {
            if (AbilityCache is null) return;
            
            foreach (int index in AbilityCache.Keys)
            {
                AbilityCache[index].ReleaseAndClean();
            }

            AbilityCache.Clear();
        }

        public bool InjectInterrupt()
        {
            if (!Executing) return false;
            activeContainer.Interrupt();
            return true;
        }

        private void ClaimActive(AbilitySpecContainer container)
        {
            switch (activationPolicy)
            {

                case EAbilityActivationPolicy.NoRestrictions:
                    break;
                case EAbilityActivationPolicy.SingleActive:
                    if (activeContainer is not null) activeContainer.Interrupt();
                    activeContainer = container;
                    break;
                case EAbilityActivationPolicy.QueueSingleActive:
                    if (activeContainer is null) activeContainer = container;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ReleaseClaim(AbilitySpecContainer container)
        {
            if (activeContainer == container) activeContainer = null;
            if (activationPolicy == EAbilityActivationPolicy.QueueSingleActive && activationQueue.Count > 0) TryActivateAbility(activationQueue.Dequeue());
        }
        
        #endregion
        
        #region Application Workers

        public SourcedModifiedAttributeValue ApplyApplicationModifications(GASComponentBase target, SourcedModifiedAttributeValue smav)
        {
            SourcedModifiedAttributeValue newValue = smav;
            foreach (AbstractApplicationWorkerScriptableObject worker in applicationWorkers)
            {
                if (!worker.ValidateWorkFor(target, newValue)) continue;
                newValue = worker.ModifyImpact(target, newValue);
            }

            return newValue;
        }
        
        #endregion
        
        #region Impact Workers
        
        public bool CommunicateAbilityImpact(AbilityImpactData impactData)
        {
            Debug.Log($"Communicated impact: {impactData}");
            
            // Allow the impact derivation to track its impact
            impactData.SourcedModifier.BaseDerivation.TrackImpact(impactData);
            impactData.SourcedModifier.BaseDerivation.RunEffectWorkers(impactData);
            
            FrameImpactData.Add(impactData);
            return true;
        }

        public void ActivateAbilityImpactWorkers()
        {
            if (FrameImpactData.Count == 0) return;

            foreach (AbilityImpactData impactData in FrameImpactData)
            {
                // Skip non-workable impact (avoids cycles)
                if (!impactData.SourcedModifier.Workable) continue;
                foreach (AbstractImpactWorkerScriptableObject worker in impactWorkers)
                {
                    if (!worker.ValidateWorkFor(impactData)) continue;
                    worker.InterpretImpact(impactData);
                }
            }

            FrameImpactData.Clear();
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

                Proxy = Spec.Base.Proxy.GenerateProxy();
                ResetTokens();
            }
            
            public bool ActivateAbility(ProxyDataPacket implicitData)
            {
                if (IsActive || IsTargeting) return false;  // Prevent reactivation mid-use
                
                Spec.Owner.AbilitySystem.ClaimActive(this);
                
                ResetTokens();
                AwaitAbility(implicitData).Forget();

                return true;
            }

            private async UniTaskVoid AwaitAbility(ProxyDataPacket data)
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
                }

                if (!targetingCancelled)
                {
                    try
                    {
                        IsActive = true;
                        Spec.Owner.AddTags(Spec.Base.Tags.ActiveGrantedTags, true);

                        await Proxy.Activate(cts.Token, data);
                    }
                    catch (OperationCanceledException)
                    {
                        // Ability in execution is interrupted (cancelled)
                    }
                    finally
                    {
                        IsActive = false;
                        Spec.Owner.RemoveTags(Spec.Base.Tags.ActiveGrantedTags);
                    }
                }
                
                ReleaseAndClean();
            }

            public void Interrupt()
            {
                if (IsTargeting) targetingCts.Cancel();
                if (IsActive) cts.Cancel();
            }

            /// <summary>
            /// Cancels the active execution of the ability
            /// </summary>
            public void ReleaseAndClean()
            {
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
                ReleaseAndClean();
                
                cts = new CancellationTokenSource();
                targetingCts = new CancellationTokenSource();
            }

            public override string ToString()
            {
                return $"{Spec} ({IsActive})";
            }
        }
    }

    public enum EAbilityActivationPolicy
    {
        NoRestrictions,  // Always able to activate any available ability
        SingleActive,  // Only able to activate one ability at a time
        QueueSingleActive  // Only able to activate one ability at a time, but subsequent activations are queued (queue is cleared in the same moment that targeting tasks are cancelled)
    }
}
