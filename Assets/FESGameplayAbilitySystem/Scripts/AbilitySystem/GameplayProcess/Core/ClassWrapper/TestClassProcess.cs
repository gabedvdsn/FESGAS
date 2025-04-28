using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public class TestClassProcess : IGameplayProcess
    {

        public void WhenInitialize()
        {
            Debug.Log($"Process initialized");
        }

        public void WhenUpdate(float lifespan)
        {
            //Debug.Log($"Process updated");// (lifetime: {lifespan})");
        }

        public void WhenReady()
        {
            Debug.Log($"Process ready");
        }

        public void WhenWait()
        {
            Debug.Log($"Process waiting");
        }

        public void WhenTerminate()
        {
            Debug.Log($"Process terminated");
        }
        
        public EProcessUpdateTiming StepTiming => EProcessUpdateTiming.Update;
        public EProcessLifecycle Lifecycle => EProcessLifecycle.SelfTerminating;

        public async UniTask RunProcess(CancellationToken token)
        {
            Debug.Log("Starting RunProcess...");

            await UniTask.Delay(2000, cancellationToken: token);

            Debug.Log("Finished RunProcess.");
        }
        public int StepPriority => 0;
    }
}
