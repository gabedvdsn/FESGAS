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
        
        private GASComponent System;
        private Dictionary<int, AbilitySpecContainer> AbilityCache;
        
        private Dictionary<AbilityScriptableObject, List<AbilitySpecContainer>> AbilityImpactSubscriptions;

        private void Awake()
        {
            System = GetComponent<GASComponent>();
            AbilityCache = new Dictionary<int, AbilitySpecContainer>();
            AbilityImpactSubscriptions = new Dictionary<AbilityScriptableObject, List<AbilitySpecContainer>>();
        }

        public void SetAbilitiesLevel(int level)
        {
            foreach (AbilitySpecContainer container in AbilityCache.Values)
            {
                container.Spec.Level = Mathf.Min(level, container.Spec.Base.MaxLevel);
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
                return true;
            }

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
            
            return container.Spec.Base.Proxy.IncludeImplicitTargeting 
                ? container.ActivateAbility(ProxyDataPacket.GenerateFrom(container.Spec, System, ESourceTarget.Target)) 
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
        
        #region Impact Workers

        public bool SubscribeToAbilityImpact(AbilitySpec subscriber, AbilityScriptableObject subscribeTo)
        {
            if (!TryGetAbilityContainer(subscriber.Base, out AbilitySpecContainer container)) return false;
            
            if (!AbilityImpactSubscriptions.ContainsKey(subscribeTo)) AbilityImpactSubscriptions[subscribeTo] = new List<AbilitySpecContainer>() { container };
            else if (!AbilityImpactSubscriptions[subscribeTo].Contains(container)) AbilityImpactSubscriptions[subscribeTo].Add(container);

            container.AddSubscription(subscribeTo);
            return true;
        }

        public bool UnsubscribeFromAbilityImpact(AbilitySpec subscriber, AbilityScriptableObject subscribeTo)
        {
            if (!TryGetAbilityContainer(subscriber.Base, out AbilitySpecContainer container)) return false;
            if (!AbilityImpactSubscriptions.ContainsKey(subscribeTo)) return false;
            if (!AbilityImpactSubscriptions[subscribeTo].Contains(container)) return false;
            
            container.RemoveSubscription(subscribeTo);
            AbilityImpactSubscriptions[subscribeTo].Remove(container);
            if (AbilityImpactSubscriptions[subscribeTo].Count == 0) AbilityImpactSubscriptions.Remove(subscribeTo);

            return true;
        }

        public bool UnsubscribeAll(AbilitySpec subscriber)
        {
            if (!TryGetAbilityContainer(subscriber.Base, out AbilitySpecContainer container)) return false;

            bool success = false;
            foreach (AbilityScriptableObject subscribeTo in AbilityImpactSubscriptions.Keys)
            {
                success = UnsubscribeFromAbilityImpact(subscriber, subscribeTo);
            }

            return success;
        }
        
        public void CommunicateAbilityImpact(AbilityImpactData impactData)
        {
            if (impactData.SourcedModifier.SourceSpec.Ability.Base.ImpactWorkers.Count == 0) return;

            foreach (AbstractAbilityImpactWorkerScriptableObject worker in impactData.SourcedModifier.SourceSpec.Ability.Base.ImpactWorkers)
            {
                worker.InterpretImpact(impactData);
            }

            if (AbilityImpactSubscriptions.TryGetValue(impactData.SourcedModifier.SourceSpec.Ability.Base, out var subscribedContainers))
            {
                foreach (AbilitySpecContainer container in subscribedContainers) container.CommunicateSubscribedAbilityImpact(impactData);
            }
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

            private List<AbilityScriptableObject> subscriptions;
            
            public AbilitySpecContainer(AbilitySpec spec)
            {
                Spec = spec;
                IsActive = false;

                Proxy = Spec.Base.Proxy.GenerateProxy();
                ResetToken();
                
                // Debug.Log($"CREATED ABILITY: {Spec.Base.Definition.Name} with proxy: {Proxy}");
            }

            public void SetupSubscriptions()
            {
                subscriptions = new List<AbilityScriptableObject>();   
                foreach (AbilityScriptableObject ability in Spec.Base.ImpactSubscriptions)
                {
                    subscriptions.Add(ability);
                    Spec.Owner.AbilitySystem.SubscribeToAbilityImpact(Spec, ability);
                }
            }

            public void AddSubscription(AbilityScriptableObject subscribeTo)
            {
                if (subscriptions.Contains(subscribeTo)) return;
                subscriptions.Add(subscribeTo);
            }

            public void RemoveSubscription(AbilityScriptableObject subscribeTo)
            {
                if (!subscriptions.Contains(subscribeTo)) return;
                subscriptions.Remove(subscribeTo);
            }
            
            public bool ActivateAbility(ProxyDataPacket implicitData)
            {
                ResetToken();
                AwaitAbility(implicitData).Forget();

                return true;
            }

            public void CommunicateSubscribedAbilityImpact(AbilityImpactData impactData)
            {
                
            }

            private async UniTaskVoid AwaitAbility(ProxyDataPacket implicitData)
            {
                IsActive = true;
                await Proxy.ActivateTargetingTask(Spec, cst.Token, implicitData);
                await Proxy.Activate(Spec, cst.Token, implicitData);
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
