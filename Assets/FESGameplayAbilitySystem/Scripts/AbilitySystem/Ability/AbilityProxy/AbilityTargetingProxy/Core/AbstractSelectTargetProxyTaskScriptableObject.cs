using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public abstract class AbstractSelectTargetProxyTaskScriptableObject : AbstractAbilityProxyTaskScriptableObject
    {
        public override void Prepare(ProxyDataPacket data)
        {
            // Hook into input handler here
            
            EnableTargetingVisualization();
        }
        
        public override void Clean(ProxyDataPacket data)
        {
            // Unhook from input handler here
            
            DisableTargetingVisualization();
        }

        protected abstract void EnableTargetingVisualization();
        protected abstract void DisableTargetingVisualization();
    }
}
