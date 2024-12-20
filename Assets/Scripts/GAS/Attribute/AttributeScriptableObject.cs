using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(menuName = "FESGAS/Attribute", fileName = "New Gameplay Attribute")]
    public class AttributeScriptableObject : ScriptableObject
    {
        public string Name;
    }
}