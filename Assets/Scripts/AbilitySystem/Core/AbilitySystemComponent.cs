using System;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [RequireComponent(typeof(AttributeSystemComponent))]
    public class AbilitySystemComponent : MonoBehaviour
    {
        [HideInInspector] public AttributeSystemComponent AttributeSystem;
        public AbilitySystemData Data;

        private void Awake()
        {
            AttributeSystem = GetComponent<AttributeSystemComponent>();
        }

        public void ApplyGameplayEffect(GameplayEffectScriptableObject gameplayEffect)
        {
            
        }
    }
}