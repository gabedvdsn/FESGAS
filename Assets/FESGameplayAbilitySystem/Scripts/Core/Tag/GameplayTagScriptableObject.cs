using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(fileName = "NewGameplayTag", menuName = "FESGAS/Tag")]
    public class GameplayTagScriptableObject : ScriptableObject
    {
        public string Name;

        public override string ToString()
        {
            return Name;
        }
    }
}
