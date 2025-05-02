using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;

namespace FESGameplayAbilitySystem
{
    public abstract class AbstractAbilityProxyTaskScriptableObject : ScriptableObject
    {
        [HideInInspector] public string ReadOnlyDescription = "This is an Ability Proxy Task (APT). APTs implement 3 methods: Prepare, Activate, and Clean. Prepare is always called before the APT is activated, and Clean is always called after the APT is finished activating, regardless of the manner the activation is resolved.";
        
        public virtual UniTask Prepare(ProxyDataPacket data) => UniTask.CompletedTask;

        public abstract UniTask Activate(ProxyDataPacket data, CancellationToken token);


        public virtual UniTask Clean(ProxyDataPacket data) => UniTask.CompletedTask;
    }

    public class ProxyDataPacket
    {
        public AbilitySpec Spec;
        
        private Dictionary<ESourceTarget, List<GASComponentBase>> SourceTargetData;
        private Dictionary<GameplayTagScriptableObject, Dictionary<ESourceTargetData, List<object>>> Payload;

        public ProxyDataPacket(AbilitySpec spec)
        {
            Spec = spec;
            SourceTargetData = new Dictionary<ESourceTarget, List<GASComponentBase>>();
            Payload = new Dictionary<GameplayTagScriptableObject, Dictionary<ESourceTargetData, List<object>>>();
        }

        #region GAS
        
        public void Add(ESourceTarget sourceTarget, GASComponentBase component, bool noDuplicates = true)
        {
            if (SourceTargetData.ContainsKey(sourceTarget))
            {
                if (noDuplicates && SourceTargetData[sourceTarget].Contains(component)) return;
                SourceTargetData[sourceTarget].Add(component);
            }
            else SourceTargetData[sourceTarget] = new List<GASComponentBase>{ component };
        }

        public bool Remove(ESourceTarget sourceTarget, GASComponentBase component)
        {
            return SourceTargetData.ContainsKey(sourceTarget) && SourceTargetData[sourceTarget].Remove(component);
        }
        
        public void AddRange(ESourceTarget sourceTarget, List<GASComponentBase> components, bool noDuplicates = true)
        {
            if (SourceTargetData.ContainsKey(sourceTarget)) SourceTargetData[sourceTarget].AddRange(noDuplicates ? components.Where(component => !SourceTargetData[sourceTarget].Contains(component)) : components);
            else SourceTargetData[sourceTarget] = components;
        }

        public ProxyDataValue<GASComponentBase> Get(ESourceTarget sourceTarget)
        {
            return !SourceTargetData.ContainsKey(sourceTarget) ? default : new ProxyDataValue<GASComponentBase>(SourceTargetData[sourceTarget]);
        }

        public ProxyDataValue<GASComponentBase> Target() => Get(ESourceTarget.Target);
        public ProxyDataValue<GASComponentBase> Source() => Get(ESourceTarget.Source);
        
        #endregion
        
        #region Payload

        public void AddPayload<T>(ESourceTargetData sourceTarget, GameplayTagScriptableObject key, T value)
        {
            if (!Payload.ContainsKey(key)) Payload[key] = new Dictionary<ESourceTargetData, List<object>>();
            Payload[key].SafeAdd(sourceTarget, value);
        }

        public void AddPayloadRange<T>(ESourceTargetData sourceTarget, GameplayTagScriptableObject key, List<T> values)
        {
            if (!Payload.ContainsKey(key)) Payload[key] = new Dictionary<ESourceTargetData, List<object>>();

            if (!Payload[key].ContainsKey(sourceTarget)) Payload[key][sourceTarget] = new List<object>();
            
            Payload[key][sourceTarget].AddRange(values);
        }

        public bool TryGetPayload<T>(ESourceTargetData sourceTarget, GameplayTagScriptableObject key, out ProxyDataValue<T> proxyDataValue)
        {
            if (!Payload.ContainsKey(key) || !Payload[key].ContainsKey(sourceTarget))
            {
                proxyDataValue = default;
                return false;
            }
            
            List<T> tObjects = new List<T>();
            foreach (object o in Payload[key][sourceTarget])
            {
                if (o is T cast) tObjects.Add(cast);
            }

            proxyDataValue = new ProxyDataValue<T>(tObjects);
            return true;
        }

        public bool TryPayloadTarget<T>(GameplayTagScriptableObject key, out ProxyDataValue<T> proxyDataValue) => TryGetPayload(ESourceTargetData.Target, key, out proxyDataValue);
        public bool TryPayloadSource<T>(GameplayTagScriptableObject key, out ProxyDataValue<T> proxyDataValue) => TryGetPayload(ESourceTargetData.Source, key, out proxyDataValue);
        
        #endregion
        
        #region Helpers

        public static ProxyDataPacket GenerateFrom(AbilitySpec spec, GASComponentBase component, ESourceTargetBoth sourceTarget)
        {
            ProxyDataPacket data = new ProxyDataPacket(spec);
            switch (sourceTarget)
            {
                case ESourceTargetBoth.Target:
                    data.Add(ESourceTarget.Target, component);
                    break;
                case ESourceTargetBoth.Source:
                    data.Add(ESourceTarget.Source, component);
                    break;
                case ESourceTargetBoth.Both:
                    data.Add(ESourceTarget.Target, component);
                    data.Add(ESourceTarget.Source, component);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sourceTarget), sourceTarget, null);
            }
            
            return data;
        }

        public void CompileWith(ProxyDataPacket other)
        {
            foreach (ESourceTarget sourceTarget in other.SourceTargetData.Keys)
            {
                AddRange(sourceTarget, other.SourceTargetData[sourceTarget]);
            }

            foreach (var key in other.Payload.Keys)
            {
                foreach (ESourceTargetData sourceTarget in other.Payload[key].Keys)
                {
                    AddPayloadRange(sourceTarget, key, other.Payload[key][sourceTarget]);
                }
            }
        }
        
        #endregion
        
        public override string ToString()
        {
            return $"Ability: {Spec}, Source: ({Source()}), Target: ({Target()})";
        }
        
    }

    public struct ProxyDataValue<T>
    {
        private List<T> Data;
        public bool Valid => Data is not null && Data.Count > 0;
        
        public ProxyDataValue(List<T> data)
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
