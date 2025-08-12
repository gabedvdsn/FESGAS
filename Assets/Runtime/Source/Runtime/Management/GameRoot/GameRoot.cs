using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public class GameRoot : GASComponentBase, IEffectDerivation
    {
        [Header("Game Root")]
        
        public static GameRoot Instance;
        
        // Useful for backend systems like observers, audio, etc...
        public List<AbstractCreateProcessProxyTask> CreateProcessTasks;
        private AbilityDataPacket NativeDataPacket;
        
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
        protected override void PrepareSystem()
        {
            TagCache = new TagCache(this);

            var systemData = ISystemData.GenerateEmpty();
            AttributeSystem.ProvidePrerequisiteData(systemData);
            AbilitySystem.ProvidePrerequisiteData(systemData);
        }

        #region Control
        
        public void Initialize()
        {
            NativeDataPacket = AbilityDataPacket.GenerateFrom
            (
                IEffectDerivation.GenerateSourceDerivation(this),
                false
            );
            
            NativeDataPacket.AddPayload(Tags.PAYLOAD_TRANSFORM, transform);
            
            RunProcessTasks(CreateProcessTasks);
        }
        
        public void RunProcessTask(AbstractCreateProcessProxyTask task)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            
            ActivateProcess(NativeDataPacket, task, cts.Token).Forget();
        }
        
        public void RunProcessTask(AbstractCreateProcessProxyTask task, AbilityDataPacket _data)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            
            ActivateProcess(_data, task, cts.Token).Forget();
        }
        
        public void RunProcessTasks(List<AbstractCreateProcessProxyTask> tasks)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            
            foreach (var task in tasks)
            {
                ActivateProcess(NativeDataPacket, task, cts.Token).Forget();
            }
        }

        public void RunProcessTasks(List<AbstractCreateProcessProxyTask> tasks, AbilityDataPacket _data)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            
            foreach (var task in tasks)
            {
                ActivateProcess(_data, task, cts.Token).Forget();
            }
        }

        private async UniTask ActivateProcess(AbilityDataPacket _data, AbstractCreateProcessProxyTask task, CancellationToken token)
        {
            task.Prepare(_data);
            await task.Activate(_data, token);
            task.Clean(_data);
        }
        
        #endregion
        
        public ISource GetOwner()
        {
            return this;
        }
        public float GetRelativeLevel()
        {
            return Identity.RelativeLevel;
        }
    }

    public class GameTagManager
    {
        
    }

    [Serializable]
    public class PremadeTagReservation
    {
        public ETagReservationPolicy Policy;
        public string Name;
        public int Key;
        public PremadeTagReservation[] Sequence;
    }

    public enum ETagReservationPolicy
    {
        String,
        Integer,
        Sequence
    }
}
