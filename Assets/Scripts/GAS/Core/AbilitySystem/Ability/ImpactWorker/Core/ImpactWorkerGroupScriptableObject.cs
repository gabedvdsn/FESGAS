using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(fileName = "IW_Group", menuName = "FESGAS/Ability/Impact Worker/Group", order = 0)]
    public class ImpactWorkerGroupScriptableObject : AbstractImpactWorkerScriptableObject
    {
        public List<AbstractImpactWorkerScriptableObject> Workers;
        
        public override void InterpretImpact(AbilityImpactData impactData)
        {
            foreach (AbstractImpactWorkerScriptableObject worker in Workers)
            {
                if (!worker.ValidateWorkFor(impactData)) continue;
                worker.InterpretImpact(impactData);
            }
        }
        
        public override bool ValidateWorkFor(AbilityImpactData impactData)
        {
            return Workers.Any(worker => worker.ValidateWorkFor(impactData));
        }
    }
}
