using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(fileName = "NewTargetingSelectTestTargetProxyTask", menuName = "FESGAS/Ability/Proxy/Targeting/Select Target Test")]
    public class SelectTestTargetProxyTaskScriptableObject : AbstractSelectTargetProxyTaskScriptableObject
    {
        public override UniTask Activate(ProxyDataPacket data, CancellationToken token)
        {
            var comps = GameObject.FindObjectsByType<GASComponent>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            var source = data.Get(ESourceTarget.Source);
            
            if (!source.Valid) return UniTask.CompletedTask;
            foreach (var comp in comps)
            {
                if (comp != source.Primary)
                {
                    data.Add(ESourceTarget.Target, comp);
                    break;
                }
            }
            
            return UniTask.CompletedTask;
        }
        
        protected override void EnableTargetingVisualization()
        {
            
        }
        
        protected override void DisableTargetingVisualization()
        {
            
        }
    }
}
