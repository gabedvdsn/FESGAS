using FESGameplayAbilitySystem.Core.AttributeSystem.AttributeWorker.Core;
using UnityEngine;

namespace FESGameplayAbilitySystem.Core.AttributeSystem.AttributeWorker.Authored
{
    [CreateAssetMenu(fileName = "FILENAME", menuName = "MENUNAME", order = 0)]
    public class PassiveAttributeWorkerScriptableObject : AbstractFocusedAttributeWorkerScriptableObject
    {

        public override void Activate(AttributeSystemComponent system)
        {
            if (!system.TryGetAttributeValue(PrimaryAttribute, out AttributeValue attributeValue)) return;
        }
    }
}
