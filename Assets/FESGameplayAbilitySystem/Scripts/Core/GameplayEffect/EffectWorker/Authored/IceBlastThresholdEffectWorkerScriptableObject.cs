using UnityEngine;

namespace FESGameplayAbilitySystem.GameplayEffect.EffectWorker.Authored
{
    [CreateAssetMenu(fileName = "FILENAME", menuName = "FESGAS/Effect/Worker/Ice Blast", order = 0)]
    public class IceBlastThresholdEffectWorkerScriptableObject : TargetAttributeRatioThresholdEffectWorkerScriptableObject
    {

        protected override void OnThresholdMet(IAttributeImpactDerivation derivation)
        {
            Debug.Log("ICE BLAST KILLLSSSS YOUUUU!!!!!!!");
        }
    }
}
