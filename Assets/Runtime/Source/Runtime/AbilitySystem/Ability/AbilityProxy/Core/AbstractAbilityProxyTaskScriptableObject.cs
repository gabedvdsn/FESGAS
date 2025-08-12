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
        [HideInInspector] public string ReadOnlyDescription = 
            "This is an Ability Proxy Task (APT). APTs implement 3 methods: Prepare, Activate, and Clean. " +
            "Prepare is always called before the APT is activated, and Clean is always called after the " +
            "APT is finished activating, regardless of the manner the activation is resolved.";

        /// <summary>
        /// Determines whether another ability proxy can be active at the same time as the proxy containing this task.
        /// For example, a proxy with some animation events has a critical section, and another proxy with a critical section must not interrupt the conclusion of the animation (and the injections relevant to the animation).
        /// If a proxy with a critical section is active, no other proxy with a critical section can be active.
        /// </summary>
        public abstract bool IsCriticalSection { get; }
        
        #region Task Methods
        
        public virtual void Prepare(AbilityDataPacket data)
        {
            
        }

        public abstract UniTask Activate(AbilityDataPacket data, CancellationToken token);


        public virtual void Clean(AbilityDataPacket data)
        {
            
        }
        
        #endregion
        
        #region Payload Helpers

        protected bool TryGetTarget(AbilityDataPacket data, out ITarget comp, EProxyDataValueTarget target)
        {
            comp = default;
            return data.TryGet(Tags.PAYLOAD_TARGET, target, out comp);
        }
        
        protected bool TryGetTarget(AbilityDataPacket data, out ITarget comp)
        {
            comp = default;
            return data.TryGetFirst(Tags.PAYLOAD_TARGET, out comp);
        }

        protected bool TryGetTargetData(AbilityDataPacket data, out SystemComponentData targetData, EProxyDataValueTarget target)
        {
            targetData = default;
            if (!TryGetTarget(data, out var comp, target)) return false;
            targetData = comp.AsData();
            return true;
        }

        protected bool TryGetTargetData(AbilityDataPacket data, out SystemComponentData targetData)
        {
            targetData = default;
            if (!data.TryGetFirst(Tags.PAYLOAD_TARGET, out ITarget target)) return false;
            targetData = target.AsData();
            return true;
        }
        
        protected bool TryGetTargetGAS(AbilityDataPacket data, out GASComponentBase gas, EProxyDataValueTarget target)
        {
            gas = default;
            if (!TryGetTarget(data, out var comp, target)) return false;
            gas = comp.AsGAS();
            return gas is not null;
        }

        protected bool TryGetTargetGAS(AbilityDataPacket data, out GASComponentBase gas)
        {
            gas = default;
            if (!data.TryGetFirst(Tags.PAYLOAD_TARGET, out ITarget target)) return false;
            gas = target.AsGAS();
            return gas is not null;
        }
        
        #endregion
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

    public enum EPayloadAccess
    {
        User,
        System
    }

    public enum EProxyDataValueTarget
    {
        Primary,
        Any,
        Last
    }
}
