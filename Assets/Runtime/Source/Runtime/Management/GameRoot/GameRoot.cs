using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public class GameRoot : GASComponent, IEffectDerivation
    {
        [Header("Game Root")]
        
        public static GameRoot Instance;
        
        // Useful for backend systems like observers, audio, etc...
        public List<AbstractCreateProcessProxyTask> CreateProcessTasks;
        private ProxyDataPacket NativeDataPacket;
        
        [Header("Defaults")]
        
        public MonoProcessParametersScriptableObject DefaultDataParameters;

        [Space(5)] 
        
        public GameplayTagScriptableObject MasterTag;
        public GameplayTagScriptableObject AllyTag;
        public GameplayTagScriptableObject EnemyTag;

        public static GameplayTagScriptableObject GASTag => Instance.DefaultDataParameters.GAS;
        public static GameplayTagScriptableObject PositionTag => Instance.DefaultDataParameters.Position;
        public static GameplayTagScriptableObject RotationTag => Instance.DefaultDataParameters.Rotation;
        public static GameplayTagScriptableObject TransformTag => Instance.DefaultDataParameters.Transform;
        public static GameplayTagScriptableObject DerivationTag => Instance.DefaultDataParameters.Derivation;
        public static GameplayTagScriptableObject GenericTag => Instance.DefaultDataParameters.Generic;
        
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
                this, ESourceTargetExpanded.Both
            );
            
            RunProcessTasks(CreateProcessTasks);
        }
        
        public void RunProcessTask(AbstractCreateProcessProxyTask task)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            
            ActivateProcess(NativeDataPacket, task, cts.Token).Forget();
        }
        
        public void RunProcessTask(AbstractCreateProcessProxyTask task, ProxyDataPacket _data)
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

        public void RunProcessTasks(List<AbstractCreateProcessProxyTask> tasks, ProxyDataPacket _data)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            
            foreach (var task in tasks)
            {
                ActivateProcess(_data, task, cts.Token).Forget();
            }
        }

        private async UniTask ActivateProcess(ProxyDataPacket _data, AbstractCreateProcessProxyTask task, CancellationToken token)
        {
            task.Prepare(_data);
            await task.Activate(_data, token);
            task.Clean(_data);
        }
        
        #endregion

        public override void WhenUpdate(ProcessRelay relay)
        {
            transform.Rotate(Vector3.up * (25f * Time.deltaTime));
        }
        
        #region Effect Derivation
        
        public GASComponentBase GetOwner()
        {
            return this;
        }
        public List<GameplayTagScriptableObject> GetContextTags()
        {
            return new List<GameplayTagScriptableObject>();
        }
        public GameplayTagScriptableObject GetAssetTag()
        {
            return Identity.NameTag;
        }
        public int GetLevel()
        {
            return Identity.Level;
        }
        public void SetLevel(int level)
        {
            Identity.Level = level;
        }
        public float GetRelativeLevel()
        {
            return Identity.Level / (float)Identity.MaxLevel;
        }
        public string GetName()
        {
            return Identity.DistinctName;
        }
        public GameplayTagScriptableObject GetAffiliation()
        {
            return MasterTag;
        }
        #endregion
    }
}
