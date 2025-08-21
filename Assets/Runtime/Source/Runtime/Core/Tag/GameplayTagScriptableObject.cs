using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(fileName = "TAG_", menuName = "FESGAS/Tag")]
    public class GameplayTagScriptableObject : ScriptableObject, ITag
    {
        public string Name;

        public override string ToString()
        {
            return Name;
        }
        public bool Equals(ITag other)
        {
            return (GameplayTagScriptableObject)other == this;
        }
    }
}
