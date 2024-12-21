using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [RequireComponent(typeof(GASComponent))]
    public class AbilitySystemComponent : MonoBehaviour
    {
        [Header("Ability System")]
        
        public int MaxAbilities = 5;
        
        private GASComponent System;
        private Dictionary<int, AbilitySpecContainer> AbilityCache;

        private void Awake()
        {
            System = GetComponent<GASComponent>();
            AbilityCache = new Dictionary<int, AbilitySpecContainer>();
        }
        
        #region Ability Managing

        public bool CanGiveAbility(AbilityScriptableObject ability) => HasRoomInCache() && !HasAbility(ability);
        
        public bool HasRoomInCache() => AbilityCache.Count < MaxAbilities;
        
        public bool HasAbility(AbilityScriptableObject ability)
        {
            return AbilityCache.Values.Any(c => c.Spec.Base == ability);
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

        public bool CanActivateAbility(int index, GASComponent target)
        {
            return AbilityCache.TryGetValue(index, out AbilitySpecContainer container) && container.Spec.ValidateActivationRequirements(target);
        }
        
        public bool ActivateAbility(int index, Vector3 position)
        {
            if (!CanActivateAbility(index)) return false;
            AbilitySpecContainer container = AbilityCache[index];

            container.ActivateAbility(position);
            return true;
        }
        
        public bool ActivateAbility(int index, GASComponent target)
        {
            /*Debug.Log(AbilityCache.TryGetValue(index, out AbilitySpecContainer _container));
            Debug.Log(_container.Spec.ValidateActivationRequirements());
            Debug.Log(!(_container.Spec.GetCooldown().DurationRemaining > 0f));
            Debug.Log(_container.Spec.CanCoverCost());
            Debug.Log(_container.Spec.Base.Tags.ValidateSourceRequirements(_container.Spec.Owner));
            */
            
            if (!CanActivateAbility(index, target)) return false;

            AbilitySpecContainer container = AbilityCache[index];
            Debug.Log("Activating ability");
            container.ActivateAbility(target);
            return true;
        }
        
        #endregion
        
        private class AbilitySpecContainer
        {
            public AbilitySpec Spec;
            public bool IsActive;
            
            public AbilityProxy Proxy;
            private CancellationToken token;

            public AbilitySpecContainer(AbilitySpec spec)
            {
                Spec = spec;
                IsActive = false;

                Proxy = Spec.Base.Proxy.GenerateProxy();
                token = Spec.Owner.GetCancellationTokenOnDestroy();

                Debug.Log($"CREATED ABILITY: {Spec.Base.Definition.Name} with proxy: {Proxy}");
            }

            public void ActivateAbility(Vector3 position)
            {
                AwaitAbility(position).Forget();
            }

            private async UniTaskVoid AwaitAbility(Vector3 position)
            {
                IsActive = true;
                await Proxy.Activate(Spec, position, token);
                IsActive = false;
            }
            
            public void ActivateAbility(GASComponent target)
            {
                AwaitAbility(target).Forget();
            }

            private async UniTaskVoid AwaitAbility(GASComponent target)
            {
                IsActive = true;
                await Proxy.Activate(Spec, target, token);
                IsActive = false;
            }
        }
    }
}
