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
        
        // Useful for backend systems like observers, audio, etc...
        public List<AbstractCreateProcessProxyTask> CreateProcessTasks;
        private ProxyDataPacket NativeDataPacket;
        
        [Header("Defaults")]
        
        public MonoProcessParametersScriptableObject DefaultDataParameters;
        
        protected override void Awake()
        {
            if (Instance is not null && Instance != this)
            {
                Debug.Log($"woah buddy");
                Destroy(gameObject);
            }

            Instance = this;
            
            base.Awake();
            
            // Self initialize when bootstrapper is null
            if (Bootstrapper.Instance is null) Initialize();
        }
        
        #region Control
        
        public void Initialize()
        {
            NativeDataPacket = ProxyDataPacket.GenerateFrom
            (
                IEffectDerivation.GenerateSourceDerivation(this),
                this, ESourceTargetBoth.Both
            );
            
            RunProcessTasks(CreateProcessTasks);
        }
        
        public void RunProcessTask(AbstractCreateProcessProxyTask task)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            
            ActivateProcess(NativeDataPacket, task, cts.Token).Forget();
        }
        
        public void RunProcessTask(AbstractCreateProcessProxyTask task, ProxyDataPacket data)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            
            ActivateProcess(data, task, cts.Token).Forget();
        }
        
        public void RunProcessTasks(List<AbstractCreateProcessProxyTask> tasks)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            
            foreach (var task in tasks)
            {
                ActivateProcess(NativeDataPacket, task, cts.Token).Forget();
            }
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
        
        #endregion

        public override void WhenUpdate(ProcessRelay relay)
        {
            transform.Rotate(Vector3.up * (25f * Time.deltaTime));
        }
    }
}
