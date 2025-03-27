using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(fileName = "PTTarget_SelectPosition_", menuName = "FESGAS/Ability/Task/Targeting/Select Position")]
    public class SelectPositionProxyTask : AbstractSelectTargetProxyTaskScriptableObject
    {

        public override UniTask Activate(ProxyDataPacket data, CancellationToken token)
        {
            throw new System.NotImplementedException();
        }
        protected override void EnableTargetingVisualization()
        {
            throw new System.NotImplementedException();
        }
        protected override void DisableTargetingVisualization()
        {
            throw new System.NotImplementedException();
        }
    }
}
