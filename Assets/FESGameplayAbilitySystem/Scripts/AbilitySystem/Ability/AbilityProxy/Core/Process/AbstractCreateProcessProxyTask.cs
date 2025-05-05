using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public abstract class AbstractCreateProcessProxyTask : AbstractAbilityProxyTaskScriptableObject
    {
        [Header("Create Process")] 
        
        public bool UseDefaultParameters = true;
        public MonoProcessParametersScriptableObject Parameters;
    }
}
