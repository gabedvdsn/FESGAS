using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(fileName = "MPP_", menuName = "FESGAS/Process/Parameters")]
    public class MonoProcessParametersScriptableObject : ScriptableObject
    {
        /*
         * These are the 6 consistent tags across any MonoProcess. In general, they are useful for:
         * Generic: Useful information that falls outside the scope of other parameter tags
         * GAS: Information regarding the source/target GAS components
         * Position: Information regarding initial position
         * Rotation: Information regarding initial rotation
         * Transform: Information regarding the process' parent transform
         * Derivation: Information regarding the process' derivation
         */
        public GameplayTagScriptableObject Generic;
        public GameplayTagScriptableObject GAS;
        public GameplayTagScriptableObject Position;
        public GameplayTagScriptableObject Rotation;
        public GameplayTagScriptableObject Transform;
        public GameplayTagScriptableObject Derivation;
    }
}
