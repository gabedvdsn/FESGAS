using System;
using System.Collections.Generic;
using System.Linq;
using Codice.Client.BaseCommands;
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
        protected Dictionary<ITag, List<object>> Payload = new(new TagComparer());

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
            data.AddPayload(ITag.Get(TagChannels.PAYLOAD_TRANSFORM), GameRoot.Instance.transform);
            return data;
        }

        public static ProcessDataPacket RootDefault(IGameplayProcessHandler handler)
        {
            var data = new ProcessDataPacket(handler);
            data.AddPayload(ITag.Get(TagChannels.PAYLOAD_TRANSFORM), GameRoot.Instance.transform);
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
            data.AddPayload(ITag.Get(TagChannels.PAYLOAD_TRANSFORM), GameRoot.Instance.transform);
            return data;
        }

        public static ProcessDataPacket RootLocal(MonoBehaviour obj, IGameplayProcessHandler handler)
        {
            var data = new ProcessDataPacket(handler);
            
            if (obj.GetComponentInParent<GameRoot>()) return data;
            data.AddPayload(ITag.Get(TagChannels.PAYLOAD_TRANSFORM), GameRoot.Instance.transform);
            return data;
        }

        public static ProcessDataPacket LocalDefault(MonoBehaviour obj)
        {
            var data = new ProcessDataPacket();
            
            data.AddPayload(ITag.Get(TagChannels.PAYLOAD_POSITION), obj.transform.position);
            data.AddPayload(ITag.Get(TagChannels.PAYLOAD_ROTATION), obj.transform.rotation);
            data.AddPayload(ITag.Get(TagChannels.PAYLOAD_TRANSFORM), obj.transform.parent);

            return data;
        }
        
        public static ProcessDataPacket LocalDefault(MonoBehaviour obj, IGameplayProcessHandler handler)
        {
            var data = new ProcessDataPacket(handler);
            
            data.AddPayload(ITag.Get(TagChannels.PAYLOAD_POSITION), obj.transform.position);
            data.AddPayload(ITag.Get(TagChannels.PAYLOAD_ROTATION), obj.transform.rotation);
            data.AddPayload(ITag.Get(TagChannels.PAYLOAD_TRANSFORM), obj.transform.parent);

            return data;
        }

        #region Core

        public void AddPayload<T>(ITag key, T value)
        {
            if (!Payload.ContainsKey(key))
            {
                Payload[key] = new List<object>()
                {
                    value
                };
            }
            else Payload[key].Add(value);
        }
        
        public bool TryGet<T>(ITag key, EProxyDataValueTarget target, out T value)
        {
            value = default;
            
            if (!Payload.ContainsKey(key))
            {
                return false;
            }

            object o = target switch
            {
                EProxyDataValueTarget.Primary => Payload[key][0],
                EProxyDataValueTarget.Any => Payload[key].RandomChoice(),
                EProxyDataValueTarget.Last => Payload[key][^1],
                _ => throw new ArgumentOutOfRangeException(nameof(target), target, null)
            };

            if (o is not T cast) return false;
            value = cast;
            return value is not null;
        }

        public bool TryGetFirst<T>(ITag key, out T value)
        {
            value = default;
            if (!Payload.ContainsKey(key)) return false;
            
            foreach (object o in Payload[key])
            {
                if (o is not T cast) continue;
                
                value = cast;
                return true;
            }
            
            return false;
        }
        
        public bool TryGet<T>(ITag key, out DataValue<T> dataValue)
        {
            if (!Payload.ContainsKey(key))
            {
                dataValue = default;
                return false;
            }
            
            List<T> tObjects = new List<T>();
            foreach (object o in Payload[key])
            {
                if (o is T cast) tObjects.Add(cast);
            }

            dataValue = new DataValue<T>(tObjects);
            return true;
        }
        
        public bool Contains<T>(T value, ITag key)
        {
            if (!Payload.ContainsKey(key)) return false;
            
            foreach (object o in Payload[key])
            {
                if (o is T cast && cast.Equals(value)) return true;
            }

            return false;
        }
        
        #endregion
    }
}
