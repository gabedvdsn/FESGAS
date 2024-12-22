using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(fileName = "NewTargetingSelectLocationProxyTask", menuName = "FESGAS/Ability/Proxy/Targeting/Select Location")]
    public class TargetingSelectPositionProxyTaskScriptableObject : AbstractAbilityProxyTaskScriptableObject
    {

        public override UniTask Activate(AbilityProxyData data, CancellationToken token)
        {
            //
            
            return UniTask.CompletedTask;
        }
    }
}
