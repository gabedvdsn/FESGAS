using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;

namespace FESGameplayAbilitySystem
{
    public class AbilityProxy
    {
        private int StageIndex;
        private readonly AbilityProxySpecification Specification;

        private Dictionary<int, CancellationTokenSource> stageSources;
        private UniTaskCompletionSource nextStageSignal;
        private int maintainedStages;
        
        public AbilityProxy(AbilityProxySpecification specification)
        {
            StageIndex = -1;
            Specification = specification;
        }

        private void Reset()
        {
            StageIndex = -1;
            stageSources = new Dictionary<int, CancellationTokenSource>();
            maintainedStages = 0;
        }

        public void Clean()
        {
            if (stageSources is not null)
            {
                foreach (int stageIndex in stageSources.Keys)
                {
                    stageSources[stageIndex]?.Cancel();
                }

                stageSources.Clear();
            }
        }

        public async UniTask ActivateTargetingTask(CancellationToken token, AbilityDataPacket implicitData)
        {
            try
            {
                // If there is a targeting task assigned...
                if (Specification.TargetingProxy)
                {
                    Specification.TargetingProxy.Prepare(implicitData);
                    await Specification.TargetingProxy.Activate(implicitData, token);
                }
            }
            catch (OperationCanceledException)
            {
                //
            }
            finally
            {
                Specification.TargetingProxy.Clean(implicitData);
            }
        }

        public async UniTask Activate(CancellationToken token, AbilityDataPacket implicitData)
        {
            Reset();
            
            await ActivateNextStage(implicitData, token);

            await UniTask.WaitUntil(() => maintainedStages <= 0, cancellationToken: token);
        }
        
        private async UniTask ActivateNextStage(AbilityDataPacket data, CancellationToken token)
        {
            StageIndex += 1;
            if (StageIndex < Specification.Stages.Count)
            {
                try
                {
                    nextStageSignal = new UniTaskCompletionSource();

                    Specification.Stages[StageIndex].Tasks.ForEach(task => task.Prepare(data));

                    ActivateStage(Specification.Stages[StageIndex], StageIndex, data, token).Forget();
                    await nextStageSignal.Task.AttachExternalCancellation(token);
                }
                catch (OperationCanceledException)
                {
                    //
                }
                finally
                {
                    Specification.Stages[StageIndex].Tasks.ForEach(task => task.Clean(data));

                    await ActivateNextStage(data, token);
                }
            }
        }

        private async UniTask ActivateStage(AbilityProxyStage stage, int stageIndex, AbilityDataPacket data, CancellationToken token)
        {
            var stageCts = CancellationTokenSource.CreateLinkedTokenSource(token);
            stageSources[stageIndex] = stageCts;
            var stageToken = stageCts.Token;

            List<UniTask> tasks = stage.Tasks.Select(task => task.Activate(data, stageToken)).ToList();
            
            try
            {
                if (tasks.Count > 0)
                {
                    switch (stage.TaskPolicy)
                    {
                        case EAnyAllPolicy.Any:
                            await UniTask.WhenAny(tasks);
                            stageCts.Cancel();
                            break;
                        case EAnyAllPolicy.All:
                            await UniTask.WhenAll(tasks);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
            catch (OperationCanceledException)
            {
                //
            }
            finally
            {
                stageCts.Dispose();
                stageSources.Remove(stageIndex);
            
                // If this stage is not maintained (hence stageIndex == StageIndex), then set next stage signal
                if (stageIndex == StageIndex) nextStageSignal?.TrySetResult();
                else maintainedStages -= 1;
            }
        }
        
        public void Inject(EAbilityInjection injection)
        {
            switch (injection)
            {
                case EAbilityInjection.INTERRUPT:  // Handled externally via the high-level cts token
                    break;
                case EAbilityInjection.BREAK_STAGE:
                    if (StageIndex < 0 || stageSources[StageIndex] is null) break;
                    stageSources[StageIndex].Cancel();
                    break;
                case EAbilityInjection.MAINTAIN_STAGE:
                    maintainedStages += 1;
                    nextStageSignal?.TrySetResult();
                    break;
                case EAbilityInjection.STOP_MAINTAIN:
                    if (StageIndex < 0 || stageSources.Count == 0) break;
                    stageSources[stageSources.Keys.ToArray()[0]]?.Cancel();
                    break;
                case EAbilityInjection.STOP_MAINTAIN_ALL:
                    if (StageIndex < 0 || stageSources.Count == 0) break;
                    foreach (int stageIndex in stageSources.Keys) stageSources[stageIndex]?.Cancel();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(injection), injection, null);
            }
        }

        public override string ToString()
        {
            string s = $"[ PROXY ]";
            int stageIndex = 0;
            foreach (AbilityProxyStage stage in Specification.Stages)
            {
                s += $"\n\t[ {stageIndex++} ] STAGE -> {stage.TaskPolicy}";
                foreach (AbstractAbilityProxyTaskScriptableObject task in stage.Tasks)
                {
                    s += $"\n\t\t{task.name}";
                }
            }

            return s;
        }
    }
    
    public enum EAbilityInjection
    {
        // Cancel the ability runtime entirely
        INTERRUPT,  
        // Cancel the active proxy stage runtime and moves to the next
        BREAK_STAGE,  
        // Same as BREAK_STAGE BUT the active proxy stage runtime CONTINUES until a STOP_MAINTAIN/_ALL injection, or the runtime reaches its natural conclusion
        MAINTAIN_STAGE,  
        // Cancels the least recent maintained proxy stage runtime
        STOP_MAINTAIN,  
        // Cancels all maintained proxy stage runtimes
        STOP_MAINTAIN_ALL  
    }
}
