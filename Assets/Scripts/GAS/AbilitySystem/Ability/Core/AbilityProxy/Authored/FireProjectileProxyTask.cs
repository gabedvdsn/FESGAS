using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public class FireProjectileProxyTask : AbstractAbilityProxyTaskScriptableObject
    {
        public AbstractAbilityBehaviour Projectile;
        public float Speed = 10;
        
        public override UniTask Activate(AbilitySpec spec, Vector3 position, CancellationToken token)
        {
            AbstractAbilityBehaviour projectile = Instantiate(Projectile);
            projectile.Spec = spec;
            Vector3 direction = (position - projectile.transform.position).normalized;
            projectile.RB.velocity = direction * Speed;
            
            return UniTask.CompletedTask;
        }
        
        public override UniTask Activate(AbilitySpec spec, GASComponent target, CancellationToken token)
        {
            return Activate(spec, target.transform.position, token);
        }
    }
}
