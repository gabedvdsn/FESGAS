using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public class GASBootstrapper : MonoBehaviour, IGameplayProcessHandler
    {
        private void Setup()
        {
            // Process control
            if (ProcessControl.Instance is null)
            {
                GameObject processControl = new GameObject("ProcessControl");
                processControl.AddComponent<ProcessControl>();
            }
            
            
        }
        
        public bool HandlerValidateAgainst(IGameplayProcessHandler handler)
        {
            return (GASBootstrapper)handler == this;
        }
    }
}
