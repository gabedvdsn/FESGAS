using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public abstract class AbstractSelectTargetProxyTaskScriptableObject : AbstractAbilityProxyTaskScriptableObject
    {
        public override UniTask Prepare(ProxyDataPacket data, CancellationToken token)
        {
            EnableTargetingVisualization();
            return base.Prepare(data, token);
        }
        
        public override UniTask Clean(ProxyDataPacket data, CancellationToken token)
        {
            DisableTargetingVisualization();
            return base.Clean(data, token);
        }

        protected abstract void EnableTargetingVisualization();
        protected abstract void DisableTargetingVisualization();
    }
}
