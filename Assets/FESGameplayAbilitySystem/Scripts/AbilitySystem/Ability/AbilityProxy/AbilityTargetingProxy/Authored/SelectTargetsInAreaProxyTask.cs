using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(fileName = "PTTarget_SelectTargetsInArea_", menuName = "FESGAS/Ability/Task/Targeting/Select Targets In Area")]
    public class SelectTargetsInAreaProxyTask : AbstractSelectTargetProxyTaskScriptableObject
    {

        public override UniTask Activate(ProxyDataPacket data, CancellationToken token)
        {
            throw new System.NotImplementedException();
        }
        protected override bool ConnectInputHandler()
        {
            throw new System.NotImplementedException();
        }
        protected override void DisconnectInputHandler()
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
