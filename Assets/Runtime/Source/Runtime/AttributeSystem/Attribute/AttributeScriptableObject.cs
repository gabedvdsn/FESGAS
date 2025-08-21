using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(menuName = "FESGAS/Attribute/Attribute", fileName = "New Gameplay Attribute")]
    public class AttributeScriptableObject : ScriptableObject, ITag, IAttribute
    {
        public string Name;

        public string GetName()
        {
            return Name;
        }
        
        public bool Equals(IAttribute other)
        {
            var attr = (AttributeScriptableObject)other;
            return attr is not null && attr == this;
        }
        
        public bool Equals(ITag other)
        {
            var tag = (AttributeScriptableObject)other;
            return tag != null && tag == this;
        }
    }

    public class CustomAttribute : IAttribute
    {
        public string Name;
        
        public bool Equals(IAttribute other)
        {
            var attr = (CustomAttribute)other;
            return attr is not null && attr == this;
        }
        
        public string GetName()
        {
            return Name;
        }
    }

    public interface IAttribute
    {
        public bool Equals(IAttribute other);
        
        public string GetName();
        
        public string GetCacheName()
        {
            return AttributeLibrary.RefactorByNamingConvention(GetName());
        }
    }
}