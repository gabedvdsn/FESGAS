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
    public class BaseFollowingProjectile : AbstractEffectingMonoProcess, IDisjointableEntity
    {
        private ITarget target;
        private AbstractTransformPacket targetTransform;

        public override void WhenInitialize(ProcessRelay relay)
        {
            base.WhenInitialize(relay);
            
            if (!regData.TryGet(Tags.PAYLOAD_TARGET, EProxyDataValueTarget.Primary, out target)) Debug.Log($"Whelp!");
            targetTransform = target.AsTransform();

            var to = Quaternion.LookRotation(targetTransform.position - transform.position);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, to, 360);
        }
        
        public override async UniTask RunProcess(ProcessRelay relay, CancellationToken token)
        {
            target.CommunicateTargetedIntent(this);
            
            await FollowTarget(relay, token);
            
            ApplyEffects(target);
        }

        protected virtual float GetProjectileSpeed()
        {
            if (!AttributeLibrary.TryGetByName("projectile_speed", out var projSpeed)) return 10f;
            return Source.AttributeSystem.TryGetAttributeValue(projSpeed, out AttributeValue val) ? val.CurrentValue : 10f;
        }

        protected virtual async UniTask FollowTarget(ProcessRelay relay, CancellationToken token)
        {
            while (Vector3.Distance(transform.position, targetTransform.position) > .1f)
            {
                token.ThrowIfCancellationRequested();
                
                transform.position = Vector3.MoveTowards(transform.position, targetTransform.position, GetProjectileSpeed() * Time.deltaTime);
                
                await UniTask.NextFrame(token);
            }
        }
        public void WhenDisjointed(DisjointTarget disjoint)
        {
            target = disjoint;
            targetTransform = disjoint.AsTransform();
        }
        public ITarget GetTarget()
        {
            return target;
        }
    }

    public interface IDisjointableEntity
    {
        public void WhenDisjointed(DisjointTarget placeholder);
        public ITarget GetTarget();
    }
}
