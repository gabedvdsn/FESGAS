using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public abstract class AbstractClassProcess : IGameplayProcess
    {
        protected bool processActive;
        
        public abstract void WhenInitialize(ProcessRelay relay);
        
        public abstract void WhenUpdate(EProcessUpdateTiming timing, ProcessRelay relay);

        /// <summary>
        /// Called via Step in ProcessControl as determined by the process's StepUpdateTiming
        /// </summary>
        /// <param name="relay">Process Relay</param>
        public abstract void WhenUpdate(ProcessRelay relay);

        /// <summary>
        /// Called via ProcessControl when the process is set to Waiting
        /// </summary>
        /// <param name="relay">Process Relay</param>
        public virtual void WhenWait(ProcessRelay relay)
        {
            processActive = false;
        }

        /// <summary>
        /// Called via ProcessControl when the process is set to Terminated
        /// </summary>
        /// <param name="relay">Process Relay</param>
        public virtual void WhenTerminate(ProcessRelay relay)
        {
            processActive = false;
        }
        
        public void WhenTerminateSafe(ProcessRelay relay)
        {
            processActive = false;
        }

        /// <summary>
        /// Called via ProcessControl when the process is set to Running
        /// </summary>
        /// <param name="relay">Process Relay</param>
        /// <param name="token">Cancellation token</param>
        /// <returns></returns>
        public abstract UniTask RunProcess(ProcessRelay relay, CancellationToken token);

        public bool TryGetProcess<T>(out T process)
        {
            if (this is T cast)
            {
                process = cast;
                return true;
            }

            process = default;
            return false;
        }
        
        public bool IsInitialized => true;
        public string ProcessName => "AnonymousClassProcess";
        public virtual EProcessUpdateTiming StepTiming => EProcessUpdateTiming.Update;
        public virtual EProcessLifecycle Lifecycle => EProcessLifecycle.SelfTerminating;
        
        public virtual int StepPriority => 0;
    }
}
