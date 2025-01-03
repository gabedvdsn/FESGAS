using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem.Core.AttributeSystem.AttributeWorker.Core
{
    [CreateAssetMenu(menuName = "FESGAS/Attribute/Worker/Group", fileName = "AW_Group")]
    public class AttributeWorkerGroupScriptableObject : AbstractAttributeWorkerScriptableObject
    {
        public List<AbstractFocusedAttributeWorkerScriptableObject> Workers;
        
        public override void Activate(AttributeSystemComponent system)
        {
            foreach (AbstractAttributeWorkerScriptableObject worker in Workers) worker.Activate(system);
        }
    }
}
