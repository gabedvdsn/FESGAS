using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace FESGameplayAbilitySystem
{
    public abstract class AbstractAbilityProxyTaskScriptableObject : ScriptableObject
    {
        [HideInInspector] public string ReadOnlyDescription = "This is an Ability Proxy Task (APT). APTs implement 3 methods: Prepare, Activate, and Clean. Prepare is always called before the APT is activated, and Clean is always called after the APT is finished activating, regardless of the manner the activation is resolved.";
        
        public virtual UniTask Prepare(ProxyDataPacket data) => UniTask.CompletedTask;

        public abstract UniTask Activate(ProxyDataPacket data, CancellationToken token);


        public virtual UniTask Clean(ProxyDataPacket data) => UniTask.CompletedTask;
    }

    public struct DataValue<T>
    {
        private List<T> Data;
        public bool Valid => Data is not null && Data.Count > 0;
        
        public DataValue(List<T> data)
        {
            Data = data;
        }

        public T Get(EProxyDataValueTarget target)
        {
            return target switch
            {

                EProxyDataValueTarget.Primary => Primary,
                EProxyDataValueTarget.Any => Any,
                EProxyDataValueTarget.Last => Last,
                _ => throw new ArgumentOutOfRangeException(nameof(target), target, null)
            };
        }

        public T Primary => Valid ? Data[0] : default;
        public T Any => Valid ? Data.RandomChoice() : default;
        public List<T> All => Valid ? Data : new List<T>();
        public List<T> AllDistinct => Valid ? All.Distinct().ToList() : new List<T>();
        public T Last => Valid ? Data[^1] : default;

        public override string ToString()
        {
            if (!Valid) return "NullProxyData";
            string s = "";
            for (int i = 0; i < Data.Count; i++)
            {
                s += $"{Data[i]}";
                if (i < Data.Count - 1) s += ", ";
            }
            
            return s;
        }
    }

    public enum ESourceTargetData
    {
        Source,
        Target,
        Data
    }

    public enum EProxyDataValueTarget
    {
        Primary,
        Any,
        Last
    }
}
