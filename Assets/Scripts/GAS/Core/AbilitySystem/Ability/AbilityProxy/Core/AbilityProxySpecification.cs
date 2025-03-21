﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace FESGameplayAbilitySystem
{
    [Serializable]
    public class AbilityProxySpecification
    {
        [Header("Targeting Instructions")]
        
        public AbstractSelectTargetProxyTaskScriptableObject TargetingProxy;
        
        [Space]
        
        public bool UseImplicitInstructions = true;
        public ESourceTargetBoth OwnerAs = ESourceTargetBoth.Source;
        
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

        public async UniTask ActivateTargetingTask(AbilitySpec spec, CancellationToken token, ProxyDataPacket implicitData)
        {
            /*ProxyDataPacket data = new ProxyDataPacket(spec);
            if (implicitData is not null) data.CompileWith(implicitData);*/

            // If there is a targeting task assigned...
            if (Specification.TargetingProxy)
            {
                await Specification.TargetingProxy.Prepare(implicitData, token);
                await Specification.TargetingProxy.Activate(implicitData, token);
                await Specification.TargetingProxy.Clean(implicitData, token);
            }

            // Debug.Log(data);
        }

        public async UniTask Activate(AbilitySpec spec, CancellationToken token, ProxyDataPacket implicitData)
        {
            Reset();
            
            /*ProxyDataPacket data = new ProxyDataPacket(spec);
            if (implicitData is not null) data.CompileWith(implicitData);*/
            
            await ActivateNextStage(implicitData, token);
        }
        
        private async UniTask ActivateNextStage(ProxyDataPacket data, CancellationToken token)
        {
            StageIndex += 1;
            if (StageIndex < Specification.Stages.Count) await ActivateStage(Specification.Stages[StageIndex], data, token);
        }

        private async UniTask ActivateStage(AbilityProxyStage stage, ProxyDataPacket data, CancellationToken token)
        {
            // Prepare all tasks
            await UniTask.WhenAll(stage.Tasks.Select(task => task.Prepare(data, token)));
            
            using (var stageCts = CancellationTokenSource.CreateLinkedTokenSource(token))
            {
                CancellationToken stageToken = stageCts.Token;
                
                List<UniTask> tasks = stage.Tasks.Select(task => task.Activate(data, stageToken)).ToList();
                
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
            
            // Clean all tasks
            await UniTask.WhenAll(stage.Tasks.Select(task => task.Clean(data, token)));
            
            ActivateNextStage(data, token).Forget();
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
