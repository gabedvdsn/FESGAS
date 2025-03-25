using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public class AbilitySystemComponent : MonoBehaviour
    {
        [Header("Ability System")]
        
        public int MaxAbilities = 5;

        [Header("Application Workers")] 
        
        public List<AbstractApplicationWorkerScriptableObject> ApplicationWorkers;
        
        [Header("Impact Workers")]
        
        public List<AbstractImpactWorkerScriptableObject> ImpactWorkers;

        private GASComponent System;
        private Dictionary<int, AbilitySpecContainer> AbilityCache;
        private List<AbilityImpactData> FrameImpactData;

        public void Initialize(GASComponent system)
        {
            System = system;
            AbilityCache = new Dictionary<int, AbilitySpecContainer>();
            FrameImpactData = new List<AbilityImpactData>();
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
        
        public bool HasRoomInCache() => AbilityCache.Count < MaxAbilities;
        
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
            
            return true;
        }

        public bool RevokeAbility(AbilityScriptableObject ability)
        {
            return TryGetCacheIndexOf(ability, out int removeIndex) && AbilityCache.Remove(removeIndex);
        }

        public bool SwapAbilityIndices(AbilityScriptableObject ability1, AbilityScriptableObject ability2)
        {
            if (!(TryGetCacheIndexOf(ability1, out int firstIndex) && TryGetCacheIndexOf(ability2, out int secondIndex))) return false;
            
            (AbilityCache[firstIndex], AbilityCache[secondIndex]) = (AbilityCache[secondIndex], AbilityCache[firstIndex]);
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

        #endregion

        #region Ability Handling

        public bool CanActivateAbility(int index)
        {
            return AbilityCache.TryGetValue(index, out AbilitySpecContainer container) && container.Spec.ValidateActivationRequirements();
        }

        public bool TryActivateAbility(int abilityIndex)
        {
            if (!CanActivateAbility(abilityIndex)) return false;
            AbilitySpecContainer container = AbilityCache[abilityIndex];

            container.Spec.ApplyUsageEffects();
            
            return container.Spec.Base.Proxy.UseImplicitInstructions 
                ? container.ActivateAbility(ProxyDataPacket.GenerateFrom(container.Spec, System, container.Spec.Base.Proxy.OwnerAs)) 
                : container.ActivateAbility(null);
        }

        public bool TryActivateAbility(AbilitySpec spec)
        {
            foreach (int abilityIndex in AbilityCache.Keys)
            {
                if (AbilityCache[abilityIndex].Spec != spec) continue;
                return TryActivateAbility(abilityIndex);
            }

            return false;
        }

        private void ClearAbilityCache()
        {
            foreach (int index in AbilityCache.Keys)
            {
                AbilityCache[index].CleanToken();
            }

            AbilityCache.Clear();
        }
        
        #endregion
        
        #region Application Workers

        public SourcedModifiedAttributeValue ApplyApplicationModifications(GASComponent target, SourcedModifiedAttributeValue smav)
        {
            SourcedModifiedAttributeValue newValue = smav;
            foreach (AbstractApplicationWorkerScriptableObject worker in ApplicationWorkers)
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
            impactData.SourcedModifier.BaseDerivation.TrackImpact(impactData.RealImpact);

            if (!ValidateWorkFor(impactData)) return false; 
            
            FrameImpactData.Add(impactData);
            return true;
        }

        private bool ValidateWorkFor(AbilityImpactData impactData)
        {
            if (ImpactWorkers.Count == 0) return false;
            foreach (AbstractImpactWorkerScriptableObject worker in ImpactWorkers)
            {
                if (!worker.ValidateWorkFor(impactData)) return false;
            }

            return true;
        }

        public void ActivateAbilityImpactWorkers()
        {
            if (FrameImpactData.Count == 0) return;

            foreach (AbilityImpactData impactData in FrameImpactData)
            {
                // Skip non-workable impact (avoids cycles)
                if (!impactData.SourcedModifier.Workable) continue;
                foreach (AbstractImpactWorkerScriptableObject worker in ImpactWorkers)
                {
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
            public bool IsActive;
            
            public AbilityProxy Proxy;
            private CancellationTokenSource cst;
            
            public AbilitySpecContainer(AbilitySpec spec)
            {
                Spec = spec;
                IsActive = false;

                Proxy = Spec.Base.Proxy.GenerateProxy();
                ResetToken();
            }
            
            public bool ActivateAbility(ProxyDataPacket implicitData)
            {
                ResetToken();
                AwaitAbility(implicitData).Forget();

                return true;
            }

            private async UniTaskVoid AwaitAbility(ProxyDataPacket data)
            {
                IsActive = true;
                await Proxy.ActivateTargetingTask(Spec, cst.Token, data);
                await Proxy.Activate(Spec, cst.Token, data);
                IsActive = false;
            }

            public void InterruptAbility()
            {
                if (!IsActive) return;
                cst?.Cancel();
            }

            public void CleanToken()
            {
                InterruptAbility();
                cst?.Dispose();
            }

            public void ResetToken()
            {
                CleanToken();
                cst = new CancellationTokenSource();
            }
        }
    }
}
