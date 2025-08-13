using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(fileName = "PT_DisjointSelf", menuName = "FESGAS/Ability/Task/Disjoint Self")]
    public class DisjointSelfProxyTask : AbstractAbilityProxyTaskScriptableObject
    {
        public override bool IsCriticalSection => false;
        public override UniTask Activate(AbilityDataPacket data, CancellationToken token)
        {
            var target = data.Spec.GetOwner();
            target.OnDisjoint(DisjointTarget.Generate(target));
            
            return UniTask.CompletedTask;
        }
    }
}
