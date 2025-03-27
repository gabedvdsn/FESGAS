using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(fileName = "AAW_Group", menuName = "FESGAS/Ability/Application Worker/Group", order = 0)]
    public class ApplicationWorkerGroupScriptableObject : AbstractApplicationWorkerScriptableObject
    {
        [Header("Application Worker Group")]
        
        public List<AbstractApplicationWorkerScriptableObject> Workers;
        
        public override SourcedModifiedAttributeValue ModifyImpact(GASComponentBase target, SourcedModifiedAttributeValue smav)
        {
            SourcedModifiedAttributeValue newValue = smav;
            foreach (AbstractApplicationWorkerScriptableObject worker in Workers)
            {
                if (!worker.ValidateWorkFor(target, smav)) continue;
                newValue = worker.ModifyImpact(target, newValue);
            }

            return newValue;
        }
        
        public override bool ValidateWorkFor(GASComponentBase target, SourcedModifiedAttributeValue smav)
        {
            return Workers.Any(worker => worker.ValidateWorkFor(target, smav));
        }
    }
}
