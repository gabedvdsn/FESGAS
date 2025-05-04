using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public class TestClassProcess : IGameplayProcess
    {

        public void WhenInitialize(ProcessRelay relay)
        {
            Debug.Log($"Process initialized");
        }

        public void WhenUpdate(ProcessRelay relay)
        {
            //Debug.Log($"Process updated");// (lifetime: {lifespan})");
        }

        public void WhenWait(ProcessRelay relay)
        {
            Debug.Log($"Process waiting");
        }

        public void WhenTerminate(ProcessRelay relay)
        {
            Debug.Log($"Process terminated");
        }

        public string ProcessName => "AnonymousClassProcess";
        public EProcessUpdateTiming StepTiming => EProcessUpdateTiming.Update;
        public EProcessLifecycle Lifecycle => EProcessLifecycle.SelfTerminating;

        public async UniTask RunProcess(ProcessRelay relay, CancellationToken token)
        {
            Debug.Log("Starting RunProcess...");

            await UniTask.Delay(2000, cancellationToken: token);

            Debug.Log("Finished RunProcess.");
        }
        public int StepPriority => 0;
    }
}
