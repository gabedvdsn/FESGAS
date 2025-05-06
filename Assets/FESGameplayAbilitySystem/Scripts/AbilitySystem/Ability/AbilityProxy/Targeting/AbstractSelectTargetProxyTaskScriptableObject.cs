using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public abstract class AbstractSelectTargetProxyTaskScriptableObject : AbstractAbilityProxyTaskScriptableObject
    {
        public override UniTask Prepare(ProxyDataPacket data)
        {
            // Hook into input handler here
            if (!ConnectInputHandler())
            {
                if (data.Spec.GetOwner().AbilitySystem.InjectInterrupt()) return UniTask.CompletedTask;
            }
            
            EnableTargetingVisualization();
            
            return base.Prepare(data);
        }
        
        public override UniTask Clean(ProxyDataPacket data)
        {
            DisableTargetingVisualization();
            
            // Unhook from input handler here
            DisconnectInputHandler();
            
            return base.Clean(data);
        }

        protected abstract bool ConnectInputHandler();
        protected abstract void DisconnectInputHandler();
        
        protected abstract void EnableTargetingVisualization();
        protected abstract void DisableTargetingVisualization();
    }
}
