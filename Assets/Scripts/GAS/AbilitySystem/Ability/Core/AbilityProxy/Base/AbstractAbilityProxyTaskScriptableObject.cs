using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace FESGameplayAbilitySystem
{
    public abstract class AbstractAbilityProxyTaskScriptableObject : ScriptableObject
    {
        public virtual async UniTask Activate(AbilitySpec spec, Vector3 position, CancellationToken token)
        {
            await UniTask.CompletedTask;
        }

        public virtual async UniTask Activate(AbilitySpec spec, GASComponent target, CancellationToken token)
        {
            {
                await UniTask.CompletedTask;
            }
        }
    }
}
