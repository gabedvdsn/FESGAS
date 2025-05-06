using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public class TrackingProjectileMonoProcess : LazyMonoProcess
    {
        private Transform target;

        public override void WhenInitialize(ProcessRelay relay)
        {
            base.WhenInitialize(relay);
            
            /*if (!data.TryGetPayload<Transform>(ESourceTargetData.Target,
                    data.TransformTag, EProxyDataValueTarget.Primary, out target))
            {
                Debug.Log($"whelp");
            }*/
        }

        public override void WhenUpdate(ProcessRelay relay)
        {
            transform.position = Vector3.Lerp(transform.position, Vector3.up * 1000, relay.Runtime / 25f);
        }
    }
}
