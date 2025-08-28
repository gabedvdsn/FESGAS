using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FESGameplayAbilitySystem
{
    public readonly struct Tag : IHasReadableDefinition, IEquatable<Tag>
    {
        public readonly string Name;
        private readonly int key;
        
        private Tag(int key, string name)
        {
            this.key = key;
            Name = name;
        }

        public static Tag Generate(int key, string name)
        {
            return new Tag(key, name);
        }
        
        public static bool operator == (Tag a, Tag b)
        {
            return a.Equals(b);
        }
        
        public static bool operator !=(Tag a, Tag b)
        {
            return !(a == b);
        }

        #region Internal
        
        public string GetName()
        {
            return $"{key}";
        }
        public string GetDescription()
        {
            return "";
        }
        public Sprite GetPrimaryIcon()
        {
            return null;
        }
        
        public bool Equals(Tag other)
        {
            return other.key == key;
        }
        
        public override bool Equals(object obj) => obj is Tag other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(key, Name);

        #endregion
    }
}
