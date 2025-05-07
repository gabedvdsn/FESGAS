using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(fileName = "TPTarget_Position", menuName = "FESGAS/Ability/Targeting/Select Position")]
    public class SelectPositionTargetProxyTask : AbstractTargetingProxyTaskScriptableObject
    {

        public override async UniTask Activate(ProxyDataPacket data, CancellationToken token)
        {
            while (true)
            {
                if (Input.GetKeyDown(KeyCode.Escape)) break;
                if (Input.GetMouseButtonDown(0))
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity))
                    {
                        data.AddPayload(ESourceTargetData.Data, GameRoot.PositionTag, hitInfo.point);
                        break;
                    }
                }
                
                await UniTask.NextFrame(token);
            }
            await UniTask.CompletedTask;
        }
    }
}
