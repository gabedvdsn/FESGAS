using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(menuName = "FESGAS/Process/MonoBehaviour/Test", fileName = "MPD_Test_")]
    public class TestMonoProcessScriptableObject : AbstractMonoProcessDataScriptableObject
    {

        public override MonoGameplayProcess WhenInitialize(Vector3 initialPosition, Quaternion initialRotation, Transform parentTransform, ProcessRelay relay)
        {
            return InstantiateMono(initialPosition, initialRotation, parentTransform);
        }
        public override void WhenUpdate(MonoGameplayProcess mono, ProcessRelay relay)
        {
            mono.transform.Rotate(Vector3.up * (relay.Runtime * .25f));
        }
        public override void WhenWait(MonoGameplayProcess mono, ProcessRelay relay)
        {
            
        }
        public override void WhenPause(MonoGameplayProcess mono, ProcessRelay relay)
        {
            
        }
        public override void WhenTerminate(MonoGameplayProcess mono, ProcessRelay relay)
        {
            Destroy(mono.gameObject);
        }
        public override async UniTask RunProcess(MonoGameplayProcess mono, ProcessRelay relay, CancellationToken token)
        {
            int delay = 5000 - Mathf.FloorToInt(relay.Runtime * 1000);
            await UniTask.Delay(delay, cancellationToken: token);
        }
    }
}
