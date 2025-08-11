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
            if (!data.TryGet(ITag.Get(TagChannels.PAYLOAD_SOURCE), EProxyDataValueTarget.Primary, out ISource source))
            {
                return UniTask.CompletedTask;
            }

            var gas = source.AsGAS();
            
            foreach (var comp in comps)
            {
                if (comp != gas && comp != GameRoot.Instance)
                {
                    data.AddPayload(ITag.Get(TagChannels.PAYLOAD_TARGET), comp);
                    break;
                }
            }
            
            return UniTask.CompletedTask;
        }
    }
}
