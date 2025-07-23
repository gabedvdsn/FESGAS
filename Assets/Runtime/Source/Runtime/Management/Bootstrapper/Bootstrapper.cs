using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;
using Debug = System.Diagnostics.Debug;
using Object = System.Object;

namespace FESGameplayAbilitySystem
{
    public class Bootstrapper : MonoBehaviour, IGameplayProcessHandler
    {
        public static Bootstrapper Instance;
        
        [Header("Gameplay Bootstrapper")]
        
        public ProcessControl ProcessControlPrefab;
        public GameRoot GameRootPrefab;

        public Dictionary<int, ProcessRelay> Relays;
        
        private void Awake()
        {
            if (Instance is not null && Instance != this) Destroy(gameObject);
            Instance = this;
            
            DontDestroyOnLoad(gameObject);
            
            if (ProcessControl.Instance is null) Instantiate(ProcessControlPrefab);
            // if (GameRoot.Instance is null) Instantiate(GameRootPrefab);

            var data = ProxyDataPacket.GenerateNull();


            Debug.Assert(ProcessControl.Instance != null, "ProcessControl.Instance != null");
            ProcessControl.Instance.Register(GameRootPrefab, data, out _);

            Initialize();
            
            // Game initialization happens here after bootstrapping is complete
            GameRoot.Instance.Initialize();
        }

        private void Initialize()
        {
            // Any further bootstrap initialization here   
        }
        
        public bool HandlerValidateAgainst(IGameplayProcessHandler handler)
        {
            return (Bootstrapper)handler == this;
        }

        public bool HandlerProcessIsSubscribed(ProcessRelay relay)
        {
            return true;
        }

        public void HandlerSubscribeProcess(ProcessRelay relay)
        {
            Relays[relay.CacheIndex] = relay;
        }
        public bool HandlerVoidProcess(int processIndex)
        {
            return Relays.Remove(processIndex);
        }
    }
}
