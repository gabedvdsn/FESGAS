using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [Serializable]
    public class AbilityProxySpecification
    {
        [Header("Targeting Instructions")]
        
        public AbstractTargetingProxyTaskScriptableObject TargetingProxy;
        
        [Space]
        
        public bool UseImplicitInstructions = true;
        public ESourceTargetExpanded OwnerAs = ESourceTargetExpanded.Source;
        
        [Header("Proxy Stages")]
        
        public List<AbilityProxyStage> Stages;

        public AbilityProxy GenerateProxy()
        {
            return new AbilityProxy(this);
        }
    }
    
    public class AbilityProxy
    {
        private int StageIndex;
        private readonly AbilityProxySpecification Specification;
        
        public AbilityProxy(AbilityProxySpecification specification)
        {
            StageIndex = -1;
            Specification = specification;
        }

        private void Reset()
        {
            StageIndex = -1;
        }

        public async UniTask ActivateTargetingTask(CancellationToken token, AbilityDataPacket implicitData)
        {
            // If there is a targeting task assigned...
            if (Specification.TargetingProxy)
            {
                Specification.TargetingProxy.Prepare(implicitData);
                await Specification.TargetingProxy.Activate(implicitData, token);
                Specification.TargetingProxy.Clean(implicitData);
            }
        }

        public async UniTask Activate(CancellationToken token, AbilityDataPacket implicitData)
        {
            Reset();
            
            await ActivateNextStage(implicitData, token);
        }
        
        private async UniTask ActivateNextStage(AbilityDataPacket data, CancellationToken token)
        {
            StageIndex += 1;
            if (StageIndex < Specification.Stages.Count)
            {
                Specification.Stages[StageIndex].Tasks.ForEach(task => task.Prepare(data));
                await ActivateStage(Specification.Stages[StageIndex], data, token);
                Specification.Stages[StageIndex].Tasks.ForEach(task => task.Clean(data));

                await ActivateNextStage(data, token);
            }
        }

        private async UniTask ActivateStage(AbilityProxyStage stage, AbilityDataPacket data, CancellationToken token)
        {
            var stageCts = CancellationTokenSource.CreateLinkedTokenSource(token);
            var stageToken = stageCts.Token;
                
            List<UniTask> tasks = stage.Tasks.Select(task => task.Activate(data, stageToken)).ToList();

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

    [Serializable]
    public class AbilityProxyStage
    {
        public EAnyAllPolicy TaskPolicy;
        public List<AbstractAbilityProxyTaskScriptableObject> Tasks;
    }

    public enum EAnyAllPolicy
    {
        Any,
        All
    }
}
