using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public abstract class AbstractMonoProcessDataScriptableObject : ScriptableObject
    {
        [Header("Mono Process Data")]
        
        public MonoGameplayProcess MonoPrefab;
        
        [Space(5)]
        
        public EProcessLifecycle Lifecycle;
        public EProcessUpdateTiming StepTiming;
        public int StepPriority;
        
        // Other data shared by all Mono processes

        protected MonoGameplayProcess InstantiateMono(Vector3 initialPosition, Quaternion initialRotation, Transform parentTransform)
        {
            if (parentTransform is null) return Instantiate(MonoPrefab, initialPosition, initialRotation);
            return Instantiate(MonoPrefab, initialPosition, initialRotation, parentTransform);
        }
        
        public abstract MonoGameplayProcess WhenInitialize(Vector3 initialPosition, Quaternion initialRotation, Transform parentTransform, ProcessRelay relay);
        public abstract void WhenUpdate(MonoGameplayProcess mono, ProcessRelay relay);
        public abstract void WhenWait(MonoGameplayProcess mono, ProcessRelay relay);
        public abstract void WhenPause(MonoGameplayProcess mono, ProcessRelay relay);
        public abstract void WhenTerminate(MonoGameplayProcess mono, ProcessRelay relay);
        public abstract UniTask RunProcess(MonoGameplayProcess mono, ProcessRelay relay, CancellationToken token);    
        
    }
}
