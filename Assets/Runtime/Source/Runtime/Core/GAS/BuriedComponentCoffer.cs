using System;
using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public class BuriedComponentCoffer
    {
        private Dictionary<Type, Component> Coffer = new();

        public bool Add<T>(T arg) where T : Component
        {
            var type = typeof(T);
            if (Coffer.ContainsKey(type)) return false;
            
            Coffer[type] = arg;
            return true;
        }

        public T Get<T>() where T : Component
        {
            if (!Coffer.TryGetValue(typeof(T), out var comp)) return null;
            if (comp is T arg) return arg;

            return null;
        }
    }
}
