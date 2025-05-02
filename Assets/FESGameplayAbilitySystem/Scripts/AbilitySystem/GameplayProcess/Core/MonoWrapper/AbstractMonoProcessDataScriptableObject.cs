using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public abstract class AbstractMonoProcessDataScriptableObject : ScriptableObject
    {
        [Header("Mono Process Data")]
        
        public AbstractMonoProcess MonoPrefab;
        
        [Space(5)]
        
        public EProcessLifecycle Lifecycle;
        public EProcessUpdateTiming StepTiming;
        public int StepPriority;
        
        // Other data shared by all Mono processes

        protected AbstractMonoProcess InstantiateMono(Vector3 initialPosition, Quaternion initialRotation, Transform parentTransform)
        {
            if (parentTransform is null) return Instantiate(MonoPrefab, initialPosition, initialRotation);
            return Instantiate(MonoPrefab, initialPosition, initialRotation, parentTransform);
        }
        
        public abstract AbstractMonoProcess WhenInitialize(Vector3 initialPosition, Quaternion initialRotation, Transform parentTransform, ProcessRelay relay);
        public abstract void WhenUpdate(AbstractMonoProcess mono, ProcessRelay relay);
        public abstract void WhenWait(AbstractMonoProcess mono, ProcessRelay relay);
        public abstract void WhenPause(AbstractMonoProcess mono, ProcessRelay relay);
        public abstract void WhenTerminate(AbstractMonoProcess mono, ProcessRelay relay);
        public abstract UniTask RunProcess(AbstractMonoProcess mono, ProcessRelay relay, CancellationToken token);    
        
    }
}
