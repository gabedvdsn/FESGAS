using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(fileName = "New Gameplay Tag", menuName = "FESGAS/Tag")]
    public class GameplayTagScriptableObject : ScriptableObject
    {
        public string Name;
    }
}
