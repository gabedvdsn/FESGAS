using System.Collections.Generic;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(fileName = "ACE_DamageReduction_", menuName = "FESGAS/Attribute/Change Event/Damage Reduction")]
    public class DamageReductionAttributeChangeEvent : AbstractModifyImpactAttributeChangeEvent
    {
        public override void AttributeChangeEvent(GASComponentBase system, ref Dictionary<AttributeScriptableObject, CachedAttributeValue> attributeCache,
            SourcedModifiedAttributeCache modifiedAttributeCache)
        {
            if (!modifiedAttributeCache.AttributeIsActive(TargetAttribute)) return;
            if (!attributeCache.ContainsKey(RelativeTo)) return;
            modifiedAttributeCache.MultiplyAttenuate(TargetAttribute, ModifyType, SignPolicy, attributeCache[RelativeTo].Value * RelativeMultiplier, AllowSelfModification, ApplyAbilityContextTags, AnyContextTag);
        }
    }
}
