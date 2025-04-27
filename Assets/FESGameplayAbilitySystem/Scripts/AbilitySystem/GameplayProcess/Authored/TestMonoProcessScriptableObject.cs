using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(menuName = "FESGAS/Process/MonoBehaviour/Test", fileName = "MPD_Test_")]
    public class TestMonoProcessScriptableObject : AbstractMonoProcessDataScriptableObject
    {

        public override MonoGameplayProcess WhenInitialize(Vector3 initialPosition, Quaternion initialRotation, Transform parentTransform)
        {
            return InstantiateMono(initialPosition, initialRotation, parentTransform);
        }
        public override void WhenUpdate(MonoGameplayProcess mono, float lifespan)
        {
            mono.transform.Rotate(Vector3.up * (lifespan * .25f));
        }
        public override void WhenWait(MonoGameplayProcess mono)
        {
            
        }
        public override void WhenPause(MonoGameplayProcess mono)
        {
            
        }
        public override void WhenTerminate(MonoGameplayProcess mono)
        {
            Debug.Log($"{name} is terminated");
            Destroy(mono.gameObject);
        }
        public override async UniTask RunProcess(MonoGameplayProcess mono, CancellationToken token)
        {
            Debug.Log($"{name} is running");
            await UniTask.Delay(5000, cancellationToken: token);
            Debug.Log($"{name} finished running");
        }
    }
}
