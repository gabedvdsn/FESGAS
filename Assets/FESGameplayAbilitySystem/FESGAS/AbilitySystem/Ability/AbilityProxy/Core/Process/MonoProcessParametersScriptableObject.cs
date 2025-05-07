using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(fileName = "MPP_", menuName = "FESGAS/Process/Parameters")]
    public class MonoProcessParametersScriptableObject : ScriptableObject
    {
        public GameplayTagScriptableObject GAS;
        public GameplayTagScriptableObject Position;
        public GameplayTagScriptableObject Rotation;
        public GameplayTagScriptableObject Transform;
    }
}
