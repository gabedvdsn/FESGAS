using System.Threading;
using Cysharp.Threading.Tasks;

namespace FESGameplayAbilitySystem
{
    public abstract class AbstractProcessWrapper : IGameplayProcess
    {
        private bool _initialized;
        public ProcessRelay Relay;
        
        public abstract void InitializeWrapper();
        
        public abstract void WhenInitialize(ProcessRelay relay);
        public abstract void WhenUpdate(EProcessUpdateTiming timing, ProcessRelay relay);
        public abstract void WhenWait(ProcessRelay relay);
        public abstract void WhenTerminate(ProcessRelay relay);
        public abstract void WhenTerminateSafe(ProcessRelay relay);
        public abstract UniTask RunProcess(ProcessRelay relay, CancellationToken token);
        public abstract bool TryGetProcess<T>(out T process);
        public bool IsInitialized => _initialized;
        public abstract string ProcessName { get; }
        public abstract int StepPriority { get; }
        public abstract EProcessUpdateTiming StepTiming { get; }
        public abstract EProcessLifecycle Lifecycle { get; }
    }
}
