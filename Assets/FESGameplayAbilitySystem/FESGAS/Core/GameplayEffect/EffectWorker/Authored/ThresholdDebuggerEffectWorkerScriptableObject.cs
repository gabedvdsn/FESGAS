using UnityEngine;

namespace FESGameplayAbilitySystem.GameplayEffect.EffectWorker.Authored
{
    [CreateAssetMenu(fileName = "ThresholdDebuggerEffectWorker", menuName = "FESGAS/Effect/Worker/Threshold Debugger")]
    public class ThresholdDebuggerEffectWorkerScriptableObject : TargetAttributeThresholdEffectWorkerScriptableObject
    {
        protected override void OnThresholdMet(IAttributeImpactDerivation derivation)
        {
            Debug.Log($"Threshold is met because of {derivation}");       
        }
    }
}
