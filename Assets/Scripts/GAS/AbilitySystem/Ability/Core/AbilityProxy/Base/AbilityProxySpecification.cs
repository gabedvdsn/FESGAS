using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [Serializable]
    public class AbilityProxySpecification
    {
        public List<AbilityProxyStage> Stages;

        public AbilityProxy GenerateProxy()
        {
            return new AbilityProxy(this);
        }
    }
    
    public class AbilityProxy
    {
        private int StageIndex;
        private AbilityProxySpecification Specification;
        
        public AbilityProxy(AbilityProxySpecification specification)
        {
            StageIndex = -1;
            Specification = specification;
        }

        private void Reset()
        {
            StageIndex = -1;
        }
        
        public async UniTask Activate(AbilitySpec spec, Vector3 position, CancellationToken token)
        {
            Reset();
            await ActivateNextStage(spec, position, token);
        }

        public async UniTask Activate(AbilitySpec spec, GASComponent target, CancellationToken token)
        {
            Reset();
            await ActivateNextStage(spec, target, token);
        }

        private async UniTask ActivateNextStage(AbilitySpec spec, Vector3 position, CancellationToken token)
        {
            StageIndex += 1;
            if (StageIndex < Specification.Stages.Count) await ActivateStage(Specification.Stages[StageIndex], spec, position, token);
        }

        private async UniTask ActivateStage(AbilityProxyStage stage, AbilitySpec spec, Vector3 position, CancellationToken token)
        {
            List<UniTask> tasks = Enumerable.Select(stage.Tasks, task => task.Activate(spec, position, token)).ToList();
            foreach (AbstractAbilityProxyTaskScriptableObject task in stage.Tasks) tasks.Add(task.Activate(spec, position, token));

            switch (stage.TaskPolicy)
            {
                case AbilityProxyTaskPolicy.Any:
                    await UniTask.WhenAny(tasks);
                    break;
                case AbilityProxyTaskPolicy.All:
                    await UniTask.WhenAll(tasks);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            ActivateNextStage(spec, position, token).Forget();
        }
        
        private async UniTask ActivateNextStage(AbilitySpec spec, GASComponent target, CancellationToken token)
        {
            StageIndex += 1;
            if (StageIndex < Specification.Stages.Count) await ActivateStage(Specification.Stages[StageIndex], spec, target, token);
        }

        private async UniTask ActivateStage(AbilityProxyStage stage, AbilitySpec spec, GASComponent target, CancellationToken token)
        {
            List<UniTask> tasks = Enumerable.Select(stage.Tasks, task => task.Activate(spec, target, token)).ToList();

            switch (stage.TaskPolicy)
            {
                case AbilityProxyTaskPolicy.Any:
                    await UniTask.WhenAny(tasks);
                    break;
                case AbilityProxyTaskPolicy.All:
                    await UniTask.WhenAll(tasks);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            ActivateNextStage(spec, target, token).Forget();
        }

        public override string ToString()
        {
            string s = $"[ PROXY ]";
            int stageIndex = 0;
            foreach (AbilityProxyStage stage in Specification.Stages)
            {
                s += $"\n\t[ {stageIndex} ] STAGE -> {stage.TaskPolicy}";
                foreach (AbstractAbilityProxyTaskScriptableObject task in stage.Tasks)
                {
                    s += $"\n\t{task.name}";
                }
            }

            return s;
        }
    }

    [Serializable]
    public class AbilityProxyStage
    {
        public AbilityProxyTaskPolicy TaskPolicy;
        public List<AbstractAbilityProxyTaskScriptableObject> Tasks;
    }

    public enum AbilityProxyTaskPolicy
    {
        Any,
        All
    }
}
