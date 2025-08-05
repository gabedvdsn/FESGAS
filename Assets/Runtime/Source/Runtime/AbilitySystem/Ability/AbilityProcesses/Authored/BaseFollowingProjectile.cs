using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    /// <summary>
    /// The base class for following/tracking projectiles. 
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class BaseFollowingProjectile : AbstractEffectingMonoProcess
    {
        private GASComponentBase targetGAS;
        private Transform target;
        private Rigidbody rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        public override void WhenInitialize(ProcessRelay relay)
        {
            base.WhenInitialize(relay);
            
            if (!regData.TryGetTarget(GameRoot.GASTag, EProxyDataValueTarget.Primary, out targetGAS)) Debug.Log($"Whelp!");
            target = targetGAS.transform;

            var to = Quaternion.LookRotation(target.position - transform.position);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, to, 360);
        }
        
        public override async UniTask RunProcess(ProcessRelay relay, CancellationToken token)
        {
            await FollowTarget(relay, token);
            
            ApplyEffects(targetGAS);
        }

        protected virtual float GetProjectileSpeed()
        {
            if (!AttributeLibrary.TryGetByName("projectile_speed", out var projSpeed)) return 10f;
            return Source.AttributeSystem.TryGetAttributeValue(projSpeed, out AttributeValue val) ? val.CurrentValue : 10f;
        }

        protected virtual async UniTask FollowTarget(ProcessRelay relay, CancellationToken token)
        {
            while (Vector3.Distance(transform.position, target.position) > .1f)
            {
                token.ThrowIfCancellationRequested();
                
                transform.position = Vector3.MoveTowards(transform.position, target.position, GetProjectileSpeed() * Time.deltaTime);
                
                await UniTask.NextFrame(token);
            }
        }
    }
}
