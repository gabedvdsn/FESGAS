using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    public abstract class AbstractImpactContextApplicationWorkerScriptableObject : AbstractApplicationWorkerScriptableObject
    {
        [Header("Application Context")] 
        
        public AttributeScriptableObject AppliedAttribute;
        public EImpactTypeAny ApplicationType;
        public EEffectImpactTargetExpanded ApplicationTarget;
        [Tooltip("Validate that exclusively the modify target is modified, as opposed to itself AND the alternative (e.g. target is Current when Current AND Base are modified would NOT pass validation.")]
        public bool ApplicationTargetExclusive;
        public ESignPolicy ApplicationSign;
        public bool AllowSelfApplication;
        
        [Space(5)]
        
        public bool AnyContextTag;
        [Tooltip("The sourced ability context tag must exist in this list")]
        public List<GameplayTagScriptableObject> ApplyAbilityContextTags;

        public override bool ValidateWorkFor(GASComponentBase target, SourcedModifiedAttributeValue smav)
        {
            if (smav.Derivation.GetAttribute() != AppliedAttribute) return false;
            if (!AllowSelfApplication && smav.BaseDerivation.GetSource() == target) return false;
            if (!AnyContextTag)
            {
                var sourceContext = smav.BaseDerivation.GetContextTags();
                if (ApplyAbilityContextTags.Any(cTag => !sourceContext.Contains(cTag)))
                {
                    return false;
                }
            }
            
            return GASHelper.ValidateImpactTypes(smav.Derivation.GetImpactType(), ApplicationType)
                   && GASHelper.ValidateImpactTargets(ApplicationTarget, smav.ToAttributeValue(), ApplicationTargetExclusive)
                   && GASHelper.ValidateSignPolicy(ApplicationSign, ApplicationTarget, smav.ToAttributeValue());
        }
    }
}
