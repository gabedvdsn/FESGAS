using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    /// <summary>
    /// ProcessDataPackets (and subclasses) contain data pertaining to processes and abilities
    /// Example usage:
    ///     An ability, Cast, fires a ball that homes in on its target
    ///     Cast is the ability and via implicit data and/or targeting tasks assigns some data to the packet
    ///     The Ball is a MonoProcess and is created via ProcessControl
    ///     Before initializing the MonoProcessWrapper is passed the data packet and its initial values are set
    ///     After initializing the MonoProcess is passed the data packet
    ///     It is responsible for procuring data from the data packet
    ///         E.g. procuring the target transform via Data.TryGetPayload[Transform](Target, GameRoot.TransformParameter, Primary, out Transform value)
    ///         This procures the Transform value under the Target classification stored under the key GameRoot.TransformParameter
    /// </summary>
    public class ProcessDataPacket
    {
        protected Dictionary<GameplayTagScriptableObject, Dictionary<ESourceTargetData, List<object>>> Payload = new();

        public IGameplayProcessHandler Handler;

        public ProcessDataPacket()
        {
            Handler = GameRoot.Instance;
        }

        public ProcessDataPacket(IGameplayProcessHandler handler)
        {
            Handler = handler;
        }

        #region Core
        
        public void AddPayload(GASComponentBase component, ESourceTargetData sourceTarget, bool noDuplicates = true)
        {
            if (noDuplicates && PayloadContains(component, GameRoot.GASTag, sourceTarget)) return;
            AddPayload(GameRoot.GASTag, sourceTarget, component);
        }

        public void AddPayload<T>(GameplayTagScriptableObject key, ESourceTargetData sourceTarget, T value)
        {
            if (!Payload.ContainsKey(key)) Payload[key] = new Dictionary<ESourceTargetData, List<object>>();
            Payload[key].SafeAdd(sourceTarget, value);
            Debug.Log($"Payload added: [{key}][{sourceTarget}] => {value}<{typeof(T)}>");
        }

        public void AddPayloadRange<T>(GameplayTagScriptableObject key, ESourceTargetData sourceTarget, List<T> values)
        {
            if (!Payload.ContainsKey(key)) Payload[key] = new Dictionary<ESourceTargetData, List<object>>();

            if (!Payload[key].ContainsKey(sourceTarget)) Payload[key][sourceTarget] = new List<object>();

            Payload[key][sourceTarget].AddRange(values);
        }
        
        public bool TryGetPayload<T>(GameplayTagScriptableObject key, ESourceTargetData sourceTarget, EProxyDataValueTarget target, out T value)
        {
            value = default;
            
            if (!Payload.ContainsKey(key) || !Payload[key].ContainsKey(sourceTarget))
            {
                return false;
            }

            object o = target switch
            {
                EProxyDataValueTarget.Primary => Payload[key][sourceTarget][0],
                EProxyDataValueTarget.Any => Payload[key][sourceTarget].RandomChoice(),
                EProxyDataValueTarget.Last => Payload[key][sourceTarget][^1],
                _ => throw new ArgumentOutOfRangeException(nameof(target), target, null)
            };

            if (o is not T cast) return false;
            value = cast;
            return value is not null;
        }

        public bool TryGetPayload<T>(GameplayTagScriptableObject key, ESourceTargetData sourceTarget, out DataValue<T> dataValue)
        {
            if (!Payload.ContainsKey(key) || !Payload[key].ContainsKey(sourceTarget))
            {
                dataValue = default;
                return false;
            }
            
            List<T> tObjects = new List<T>();
            foreach (object o in Payload[key][sourceTarget])
            {
                if (o is T cast) tObjects.Add(cast);
            }

            dataValue = new DataValue<T>(tObjects);
            return true;
        }

        public bool TryGetTarget<T>(GameplayTagScriptableObject key, EProxyDataValueTarget target, out T value)
        {
            return TryGetPayload<T>(key, ESourceTargetData.Target, target, out value);
        }
        
        public bool TryGetSource<T>(GameplayTagScriptableObject key, EProxyDataValueTarget target, out T value)
        {
            return TryGetPayload<T>(key, ESourceTargetData.Source, target, out value);
        }
        
        public bool TryGetData<T>(GameplayTagScriptableObject key, EProxyDataValueTarget target, out T value)
        {
            return TryGetPayload<T>(key, ESourceTargetData.Data, target, out value);
        }
        
        public bool PayloadContains<T>(T value, GameplayTagScriptableObject key, ESourceTargetData sourceTarget)
        {
            if (!Payload.ContainsKey(key) || !Payload[key].ContainsKey(sourceTarget)) return false;
            
            foreach (var o in Payload[key][sourceTarget])
            {
                if (o is T cast && cast.Equals(value)) return true;
            }

            return false;
        }
        
        #endregion
    }
}
