using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(fileName = "New Delay Proxy Task", menuName = "FESGAS/Ability/Task/Delay")]
    public class DelayProxyTask : AbstractAbilityProxyTaskScriptableObject
    {
        [Header("Delay Task")]
        public int DelayMilliseconds;
        
        public override async UniTask Activate(AbilitySpec spec, Vector3 position, CancellationToken token)
        {
            await UniTask.Delay(DelayMilliseconds, cancellationToken: token);
            Debug.Log($"DELAY DONE {DelayMilliseconds}");
        }
        
        public override async UniTask Activate(AbilitySpec spec, GASComponent target, CancellationToken token)
        {
            await UniTask.Delay(DelayMilliseconds, cancellationToken: token);
            Debug.Log($"DELAY DONE {DelayMilliseconds}");
        }
    }
}
