﻿using System;
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

        public static ProxyDataPacket GenerateDefault(IEffectDerivation spec)
        {
            ProxyDataPacket data = new ProxyDataPacket(spec);
            return data;
        }

        public static ProxyDataPacket GenerateFrom(IEffectDerivation spec, GASComponentBase component, ESourceTargetExpanded sourceTarget)
        {
            ProxyDataPacket data = new ProxyDataPacket(spec);
            switch (sourceTarget)
            {
                case ESourceTargetExpanded.Target:
                    data.AddPayload(component, ESourceTargetData.Target);
                    break;
                case ESourceTargetExpanded.Source:
                    data.AddPayload(component, ESourceTargetData.Source);
                    break;
                case ESourceTargetExpanded.Both:
                    data.AddPayload(component, ESourceTargetData.Target);
                    data.AddPayload(component, ESourceTargetData.Source);
                    break;
                case ESourceTargetExpanded.Neither:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sourceTarget), sourceTarget, null);
            }
            
            return data;
        }

        public static ProxyDataPacket GenerateFrom(IEffectDerivation spec, GameplayTagScriptableObject tag, GASComponentBase character, ESourceTargetData target)
        {
            var data = new ProxyDataPacket(spec);
            data.AddPayload(tag, target, character);
            return data;
        }
        
    }
}
