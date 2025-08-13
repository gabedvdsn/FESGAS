using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    /// <summary>
    /// Do you need component references at the GASComponent level? Use the Buried Component Coffer!
    /// </summary>
    public class BuriedComponentCoffer
    {
        private Dictionary<int, Component> Coffer = new();
        private Dictionary<Type, List<int>> RefCoffer = new();
        
        private int nextId = 0;

        public int Add(Component comp)
        {
            int key = nextId++;
            
            Coffer[key] = comp;
            RefCoffer.SafeAdd(comp.GetType(), key);
            
            return key;
        }

        public T Get<T>(int key) where T : Component
        {
            if (Coffer.TryGetValue(key, out var comp) && comp is T arg) return arg;
            return null;
        }

        public bool TryGet<T>(int key, out T arg) where T : Component
        {
            arg = Get<T>(key);
            return arg is not null;
        }

        public List<T> Get<T>() where T : Component
        {
            return !RefCoffer.TryGetValue(typeof(T), out var indices) ? null : indices.Select(index => Coffer[index] as T).ToList();
        }

        public T GetFirst<T>() where T : Component
        {
            return !RefCoffer.TryGetValue(typeof(T), out var indices) || indices.Count == 0 ? null : Coffer[indices[0]] as T;
        }

        public bool TryGet<T>(out List<T> comps) where T : Component
        {
            comps = Get<T>();
            return comps is not null;
        }

        public bool TryGetFirst<T>(out T comp) where T : Component
        {
            comp = GetFirst<T>();
            return comp is not null;
        }
    }
}
