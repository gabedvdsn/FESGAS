using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace FESGameplayAbilitySystem
{
    public abstract class AbstractAbilityProxyTaskScriptableObject : ScriptableObject
    {
        [HideInInspector] public string ReadOnlyDescription = "This is an Ability Proxy Task";
        
        public virtual UniTask Prepare(AbilitySpec spec, Vector3 position, CancellationToken token) => UniTask.CompletedTask;
        
        public virtual UniTask Prepare(AbilitySpec spec, GASComponent target, CancellationToken token) => UniTask.CompletedTask;
        
        public abstract UniTask Activate(AbilitySpec spec, Vector3 position, CancellationToken token);

        public abstract UniTask Activate(AbilitySpec spec, GASComponent target, CancellationToken token);

        public virtual UniTask Clean(AbilitySpec spec, Vector3 position, CancellationToken token) => UniTask.CompletedTask;
        
        public virtual UniTask Clean(AbilitySpec spec, GASComponent target, CancellationToken token) => UniTask.CompletedTask;
    }
}
