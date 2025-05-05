using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(fileName = "MPP_", menuName = "FESGAS/Process/Parameters")]
    public class MonoProcessParametersScriptableObject : ScriptableObject
    {
        [Header("Mono Process Parameters")] public Quaternion Rot;
        
        public GameplayTagScriptableObject FollowTransformTag;
        public GameplayTagScriptableObject PositionTag;
        public GameplayTagScriptableObject RotationTag;
    }
}
