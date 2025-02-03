using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(fileName = "IW_Group", menuName = "FESGAS/Ability/Impact Worker/Group", order = 0)]
    public class ImpactWorkerGroupScriptableObject : AbstractImpactWorkerScriptableObject
    {
        public List<AbstractImpactWorkerScriptableObject> Workers;
        
        public override void InterpretImpact(AbilityImpactData impactData)
        {
            foreach (AbstractImpactWorkerScriptableObject worker in Workers) worker.InterpretImpact(impactData);
        }
    }
}
