using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public class GameRoot : GASComponent
    {
        [Header("Game Root")]
        
        public static GameRoot Instance;
        public new bool DontDestroyOnLoad = true;
        
        [Space]
        
        // Useful for backend systems like observers, audio, etc...
        public List<AbstractCreateProcessProxyTask> CreateProcessTasks;
        private ProxyDataPacket NativeDataPacket;
        
        [Header("Defaults")]
        
        public MonoProcessParametersScriptableObject DefaultDataParameters;
        
        protected override void Awake()
        {
            if (Instance is not null && Instance != this)
            {
                Destroy(gameObject);
            }

            Instance = this;
            
            // Only use DDoL if ProcessControl is also using
            if (DontDestroyOnLoad && ProcessControl.Instance.DontDestroyOnLoad) DontDestroyOnLoad(gameObject);
            
            base.Awake();
        }

        private void Start()
        {
            NativeDataPacket = ProxyDataPacket.GenerateFrom
            (
                IEffectDerivation.GenerateSourceDerivation(this),
                this, ESourceTargetBoth.Both
            );
            
            RunProcessTasks(CreateProcessTasks, NativeDataPacket);
        }
        
        public void RunProcessTask(AbstractCreateProcessProxyTask task, ProxyDataPacket data)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            
            ActivateProcess(data, task, cts.Token).Forget();
        }

        public void RunProcessTasks(List<AbstractCreateProcessProxyTask> tasks, ProxyDataPacket data)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            
            foreach (var task in tasks)
            {
                ActivateProcess(data, task, cts.Token).Forget();
            }
        }

        private async UniTask ActivateProcess(ProxyDataPacket data, AbstractCreateProcessProxyTask task, CancellationToken token)
        {
            task.Prepare(data);
            await task.Activate(data, token);
            task.Clean(data);
        }
    }
}
