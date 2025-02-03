using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem.Core.AbilitySystem.Ability.AbilityProxy.Subscription
{
    [CreateAssetMenu(fileName = "FILENAME", menuName = "MENUNAME", order = 0)]
    public class SubscriptionAbilityProxyTaskScriptableObject : AbstractAbilityProxyTaskScriptableObject
    {

        public override UniTask Activate(ProxyDataPacket data, CancellationToken token)
        {
            throw new System.NotImplementedException();
        }
    }
}
