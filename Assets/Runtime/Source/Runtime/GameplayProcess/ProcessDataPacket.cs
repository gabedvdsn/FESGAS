using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEditor;
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
        protected Dictionary<ITag, Dictionary<ESourceTargetData, List<object>>> Payload = new(new TagComparer());

        public IGameplayProcessHandler Handler;

        protected ProcessDataPacket()
        {
            Handler = GameRoot.Instance;
        }

        private ProcessDataPacket(IGameplayProcessHandler handler)
        {
            Handler = handler;
        }

        public static ProcessDataPacket RootDefault()
        {
            var data = new ProcessDataPacket();
            data.AddPayload(GameRoot.TransformTag, ESourceTargetData.Data, GameRoot.Instance.transform);
            return data;
        }

        public static ProcessDataPacket RootDefault(IGameplayProcessHandler handler)
        {
            var data = new ProcessDataPacket(handler);
            data.AddPayload(GameRoot.TransformTag, ESourceTargetData.Data, GameRoot.Instance.transform);
            return data;
        }
        
        /// <summary>
        /// Returns a new data packet where GameRoot is the assigned Transform IF GameRoot is not within the parental hierarchy
        /// </summary>
        /// <returns></returns>
        public static ProcessDataPacket RootLocal(MonoBehaviour obj)
        {
            var data = new ProcessDataPacket();
            
            if (obj.GetComponentInParent<GameRoot>()) return data;
            data.AddPayload(GameRoot.TransformTag, ESourceTargetData.Data, GameRoot.Instance.transform);
            return data;
        }

        public static ProcessDataPacket RootLocal(MonoBehaviour obj, IGameplayProcessHandler handler)
        {
            var data = new ProcessDataPacket(handler);
            
            if (obj.GetComponentInParent<GameRoot>()) return data;
            data.AddPayload(GameRoot.TransformTag, ESourceTargetData.Data, GameRoot.Instance.transform);
            return data;
        }

        public static ProcessDataPacket LocalDefault(MonoBehaviour obj)
        {
            var data = new ProcessDataPacket();
            
            data.AddPayload(GameRoot.PositionTag, ESourceTargetData.Data, obj.transform.position);
            data.AddPayload(GameRoot.RotationTag, ESourceTargetData.Data, obj.transform.rotation);
            data.AddPayload(GameRoot.TransformTag, ESourceTargetData.Data, obj.transform.parent);

            return data;
        }
        
        public static ProcessDataPacket LocalDefault(MonoBehaviour obj, IGameplayProcessHandler handler)
        {
            var data = new ProcessDataPacket(handler);
            
            data.AddPayload(GameRoot.PositionTag, ESourceTargetData.Data, obj.transform.position);
            data.AddPayload(GameRoot.RotationTag, ESourceTargetData.Data, obj.transform.rotation);
            data.AddPayload(GameRoot.TransformTag, ESourceTargetData.Data, obj.transform.parent);

            return data;
        }

        #region Core
        
        public void AddPayload(GASComponentBase component, ESourceTargetData sourceTarget, bool noDuplicates = true)
        {
            if (noDuplicates && PayloadContains(component, GameRoot.GASTag, sourceTarget)) return;
            AddPayload(GameRoot.GASTag, sourceTarget, component);
        }

        public void AddPayload<T>(ITag key, ESourceTargetData sourceTarget, T value)
        {
            if (!Payload.ContainsKey(key)) Payload[key] = new Dictionary<ESourceTargetData, List<object>>();
            Payload[key].SafeAdd(sourceTarget, value);
        }
        
        public bool TryGetPayload<T>(ITag key, ESourceTargetData sourceTarget, EProxyDataValueTarget target, out T value)
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

        public bool TryGetPayload<T>(ITag key, ESourceTargetData sourceTarget, out DataValue<T> dataValue)
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

        public bool TryGetTarget<T>(ITag key, EProxyDataValueTarget target, out T value)
        {
            return TryGetPayload(key, ESourceTargetData.Target, target, out value);
        }
        
        public bool TryGetSource<T>(ITag key, EProxyDataValueTarget target, out T value)
        {
            return TryGetPayload(key, ESourceTargetData.Source, target, out value);
        }
        
        public bool TryGetData<T>(ITag key, EProxyDataValueTarget target, out T value)
        {
            return TryGetPayload(key, ESourceTargetData.Data, target, out value);
        }
        
        public bool PayloadContains<T>(T value, ITag key, ESourceTargetData sourceTarget)
        {
            if (!Payload.ContainsKey(key) || !Payload[key].ContainsKey(sourceTarget)) return false;
            
            foreach (object o in Payload[key][sourceTarget])
            {
                if (o is T cast && cast.Equals(value)) return true;
            }

            return false;
        }
        
        #endregion
    }
}
