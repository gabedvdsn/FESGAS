using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(menuName = "FESGAS/Attribute/Attribute", fileName = "New Gameplay Attribute")]
    public class AttributeScriptableObject : ScriptableObject
    {
        public string Name;
        
        public override string ToString() => $"Attribute.{string.Concat(Name.Where(c => !char.IsWhiteSpace(c)))}";
    }
}