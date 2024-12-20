using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [RequireComponent(typeof(GASComponent))]
    public class AbilitySystemComponent : MonoBehaviour
    {
        private GASComponent System;

        private void Awake()
        {
            System = GetComponent<GASComponent>();
        }

        public IEnumerator ActivateAbility()
        {
            AbilitySpecContainer container = new AbilitySpecContainer();
            if (!container.TryActivateAbility(out IEnumerator abilityRoutine)) yield break;
            
            yield return abilityRoutine;

            container.IsActive = false;
        }
        
        private class AbilitySpecContainer
        {
            public AbstractAbilityScriptableObject.AbstractAbilitySpec Spec;
            public bool IsActive;

            public bool TryActivateAbility(out IEnumerator abilityRoutine)
            {
                abilityRoutine = null;
                if (!Spec.CanActivateAbility()) return false;
            
                IsActive = true;
                abilityRoutine = Spec.DoAbility();
                return true;
            }
        }
    }
}
