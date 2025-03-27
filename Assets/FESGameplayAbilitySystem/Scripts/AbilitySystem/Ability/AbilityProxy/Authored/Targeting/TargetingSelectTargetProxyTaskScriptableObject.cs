using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(fileName = "NewTargetingSelectTargetProxyTask", menuName = "FESGAS/Ability/Proxy/Targeting/Select Target")]
    public class TargetingSelectTargetProxyTaskScriptableObject : AbstractAbilityProxyTaskScriptableObject
    {

        public override async UniTask Activate(ProxyDataPacket data, CancellationToken token)
        {
            // wait for response from some cursor manager that receives mouse input and finds the selected gameobject that has a GASComponent
            // await CursorManager.Instance.SetSelectTargetObjectMode();
            // if (CursorManager.Instance.LastSelectTargetObject) data.Add(ESourceTarget.Target, CursorManager.Instance.LastSelectTargetObject);
            await UniTask.Delay(0, cancellationToken: token);
        }
    }
}
