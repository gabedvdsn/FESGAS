using System;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public class AttributeStore : MonoBehaviour
    {
        public static AttributeStore Instance;

        [Header("Stored Attributes")] 
        
        public AttributeScriptableObject Health;
        
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
        }
    }
}
