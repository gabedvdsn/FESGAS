using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;
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
            if (GameRoot.Instance is null) Instantiate(GameRootPrefab);
            
            var data = ProxyDataPacket.GenerateFrom(
                IEffectDerivation.GenerateSourceDerivation(GameRoot.Instance), 
                GameRoot.Instance, ESourceTargetBoth.Both);
            
            ProcessControl.Instance.RegisterMonoProcess(new MonoProcessPacket(GameRootPrefab), data, null);
        }
        
        public bool HandlerValidateAgainst(IGameplayProcessHandler handler)
        {
            return (Bootstrapper)handler == this;
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
