using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public struct GameplayEffectAttributeCaptureDefinition
    {
        public EGameplayEffectAttributeCaptureSource AttributeSource;
        public AttributeScriptableObject CaptureAttribute;
        public bool Snapshot;

        public GameplayEffectAttributeCaptureDefinition(AttributeScriptableObject inAttribute, EGameplayEffectAttributeCaptureSource inSource,  bool inSnapshot)
        {
            AttributeSource = inSource;
            CaptureAttribute = inAttribute;
            Snapshot = inSnapshot;
        }
    }
    
    public enum EGameplayEffectAttributeCaptureSource
    {
        Source,
        Target
    }
}
