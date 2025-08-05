using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(fileName = "TestSelectTarget_", menuName = "FESGAS/Ability/Targeting/Select Target Test")]
    public class GetTestTargetProxyTask : AbstractTargetingProxyTaskScriptableObject
    {
        public override UniTask Activate(AbilityDataPacket data, CancellationToken token)
        {
            var comps = GameObject.FindObjectsByType<GASComponentBase>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            if (!data.TryGetSource(GameRoot.GASTag, EProxyDataValueTarget.Primary, out GASComponentBase source))
            {
                return UniTask.CompletedTask;
            }
            
            foreach (var comp in comps)
            {
                if (comp != source && comp != GameRoot.Instance)
                {
                    data.AddPayload(GameRoot.GASTag, ESourceTargetData.Target, comp);
                    break;
                }
            }
            
            return UniTask.CompletedTask;
        }
    }
}
