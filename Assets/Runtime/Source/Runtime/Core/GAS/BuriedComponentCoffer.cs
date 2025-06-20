using System;
using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public class BuriedComponentCoffer
    {
        private Dictionary<int, Component> Coffer = new();
        private int nextId = 0;

        public int Register(Component comp)
        {
            int key = nextId++;
            Coffer[key] = comp;
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
    }
}
