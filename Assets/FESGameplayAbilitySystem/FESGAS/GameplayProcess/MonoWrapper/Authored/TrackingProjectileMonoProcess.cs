using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [RequireComponent(typeof(Rigidbody))]
    public class TrackingProjectileMonoProcess : LazyMonoProcess
    {
        private GASComponentBase targetGAS;
        public Transform target;
        private Rigidbody rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        public override void WhenInitialize(ProcessRelay relay)
        {
            base.WhenInitialize(relay);
            
            if (!data.TryGetTarget(GameRoot.GASTag, EProxyDataValueTarget.Primary, out targetGAS)) Debug.Log($"Whelp!");
            target = targetGAS.transform;

            var to = Quaternion.LookRotation(target.position - transform.position);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, to, 360);
        }
        
        public override async UniTask RunProcess(ProcessRelay relay, CancellationToken token)
        {
            while (Vector3.Distance(transform.position, target.position) > .1f)
            {
                token.ThrowIfCancellationRequested();

                //rb.velocity = (target.position - transform.position).normalized * (15f);
                
                transform.position = Vector3.MoveTowards(transform.position, target.position, 8 * Time.deltaTime);
                
                /*var to = Quaternion.LookRotation(target.position - transform.position);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, to, 180f * Time.deltaTime);*/

                await UniTask.NextFrame(token);
            }
        }
    }
}
