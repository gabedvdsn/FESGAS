using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(fileName = "PT_Delay_", menuName = "FESGAS/Ability/Task/Delay")]
    public class DelayProxyTask : AbstractAbilityProxyTaskScriptableObject
    {
        public int DelayMilliseconds;
        
        public override async UniTask Activate(ProxyDataPacket data, CancellationToken token)
        {
            Debug.Log($"start delay");
            await UniTask.Delay(DelayMilliseconds, cancellationToken: token);
            Debug.Log($"end delay");
        }
    }
}
