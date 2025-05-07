using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public abstract class AbstractTargetingProxyTaskScriptableObject : AbstractAbilityProxyTaskScriptableObject
    {
        public override UniTask Prepare(ProxyDataPacket data)
        {
            // Hook into input handler here
            if (!ConnectInputHandler(data))
            {
                if (data.Spec.GetOwner().AbilitySystem.InjectInterrupt()) return UniTask.CompletedTask;
            }
            
            return base.Prepare(data);
        }
        
        public override UniTask Clean(ProxyDataPacket data)
        {
            // Unhook from input handler here
            DisconnectInputHandler(data);
            
            return base.Clean(data);
        }

        /// <summary>
        /// Input handler can use data to derive visualization and validity
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected bool ConnectInputHandler(ProxyDataPacket data)
        {
            // Claim input handler
            return true;
        }
        
        protected void DisconnectInputHandler(ProxyDataPacket data)
        {
            // Release claim on input handler
        }
    }
}
