using System.Collections.Generic;
using UnityEngine;

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

            if (ProcessControl.Instance is null)
            {
                if (ProcessControlPrefab) Instantiate(ProcessControlPrefab);
                else
                {
                    var control = new GameObject("ProcessControl");
                    control.AddComponent<ProcessControl>();
                }
            }
            
            Debug.Assert(ProcessControl.Instance != null, "ProcessControl is INACTIVE");
            ProcessControl.Instance.Register(GameRootPrefab, AbilityDataPacket.GenerateNull(), out _);

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
