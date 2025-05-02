using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    
    public class MonoProcessParametersScriptableObject : ScriptableObject
    {
        [Header("Mono Process Parameters")]
        
        public GameplayTagScriptableObject FollowTransformTag;
        public GameplayTagScriptableObject PositionTag;
        public GameplayTagScriptableObject RotationTag;
    }
}
