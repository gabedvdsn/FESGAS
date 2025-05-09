using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public abstract class AbstractTargetingProxyTaskScriptableObject : AbstractAbilityProxyTaskScriptableObject
    {
        public override void Prepare(ProxyDataPacket data)
        {
            // Hook into input handler here
            if (!ConnectInputHandler(data))
            {
                // Can add logic if interrupt injection failed
                data.Spec.GetOwner().AbilitySystem.InjectInterrupt();
            }
        }
        
        public override void Clean(ProxyDataPacket data)
        {
            // Unhook from input handler here
            DisconnectInputHandler(data);
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
