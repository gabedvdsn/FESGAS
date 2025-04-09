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
                if (data.Spec.Owner.AbilitySystem.InjectInterrupt()) return UniTask.CompletedTask;
            }
            EnableTargetingVisualization();
            return base.Prepare(data);
        }
        
        public override UniTask Clean(ProxyDataPacket data)
        {
            // Unhook from input handler here
            DisconnectInputHandler();
            
            DisableTargetingVisualization();
            return base.Prepare(data);
        }

        protected abstract bool ConnectInputHandler();
        protected abstract void DisconnectInputHandler();
        
        protected abstract void EnableTargetingVisualization();
        protected abstract void DisableTargetingVisualization();
    }
}
