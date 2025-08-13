using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public class AbilityDataPacket : ProcessDataPacket
    {
        public IEffectDerivation Spec;

        private AbilityDataPacket(IEffectDerivation spec)
        {
            Spec = spec;
            Handler = spec.GetOwner();
            
            AddPayload(
                Tags.PAYLOAD_SOURCE,
                spec.GetOwner()
            );
        }

        public static AbilityDataPacket GenerateNull()
        {
            return new AbilityDataPacket(IEffectDerivation.GenerateSourceDerivation(null));
        }

        public static AbilityDataPacket GenerateFrom(IEffectDerivation spec, bool useImplicitTargeting)
        {
            AbilityDataPacket data = new AbilityDataPacket(spec);
            
            if (useImplicitTargeting)
            {
                data.AddPayload(Tags.PAYLOAD_TARGET, spec.GetOwner());
            }
            
            return data;
        }
        
        #region Common

        public bool TryGetTarget(EProxyDataValueTarget policy, out ITarget target) => TryGet(Tags.PAYLOAD_TARGET, policy, out target);
        public bool TryGetFirstTarget(out ITarget target) => TryGetFirst(Tags.PAYLOAD_TARGET, out target);

        #endregion
    }
}
