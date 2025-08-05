using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public abstract class AbstractTargetingProxyTaskScriptableObject : AbstractAbilityProxyTaskScriptableObject
    {
        public override void Prepare(AbilityDataPacket data)
        {
            // Hook into input handler here
            if (ConnectInputHandler(data)) return;
            // Can add logic if interrupt injection failed
            if (data.Spec.GetOwner().FindAbilitySystem(out var abil)) abil.Inject(EAbilityInjection.INTERRUPT);
        }
        
        public override void Clean(AbilityDataPacket data)
        {
            // Unhook from input handler here
            DisconnectInputHandler(data);
        }

        /// <summary>
        /// Input handler can use data to derive visualization and validity
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected bool ConnectInputHandler(AbilityDataPacket data)
        {
            // Claim input handler
            return true;
        }
        
        protected void DisconnectInputHandler(AbilityDataPacket data)
        {
            // Release claim on input handler
        }
    }
}
