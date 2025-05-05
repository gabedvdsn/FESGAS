using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

namespace FESGameplayAbilitySystem
{
    public class ProxyDataPacket : ProcessDataPacket
    {
        public IEffectDerivation Spec;

        private ProxyDataPacket(IEffectDerivation spec)
        {
            Spec = spec;
            Handler = spec.GetOwner();
        }

        public static ProxyDataPacket GenerateFrom(IEffectDerivation spec, GASComponentBase component, ESourceTargetBoth sourceTarget)
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
        
        public override string ToString()
        {
            return $"Ability: {Spec}, Source: ({Source()}), Target: ({Target()})";
        }
        
    }
}
