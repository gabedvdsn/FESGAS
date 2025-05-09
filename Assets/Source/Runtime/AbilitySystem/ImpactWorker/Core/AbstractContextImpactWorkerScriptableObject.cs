using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace FESGameplayAbilitySystem
{
    public abstract class AbstractContextImpactWorkerScriptableObject : AbstractImpactWorkerScriptableObject
    {
        [Header("Impact Context")]
        
        [Tooltip("The attribute must be the target attribute in the impact")]
        public AttributeScriptableObject ImpactedAttribute;
        public EImpactTypeAny ImpactType;
        public EEffectImpactTargetExpanded ImpactTarget;
        [Tooltip("Validate that exclusively the modify target is modified, as opposed to itself AND the alternative (e.g. target is Current when Current AND Base are modified would NOT pass validation.")]
        public bool ImpactTargetExclusive;
        public ESignPolicy ImpactSign;
        public bool AllowSelfImpact;
        
        [Space(5)]
        
        public bool AnyContextTag;
        [Tooltip("The sourced ability context tags (all of them) must exist in this list")]
        public List<GameplayTagScriptableObject> ImpactAbilityContextTags;
        
        [Header("Work Context")] 
        
        [Tooltip("The attribute to apply work on")]
        public AttributeScriptableObject WorkAttribute;
        public EImpactType WorkImpactType;
        public ESignPolicy WorkSignPolicy;
        
        [Space(5)]
        
        public bool WorkSameFrame = true;
        
        public override void InterpretImpact(AbilityImpactData impactData)
        {
            PerformImpactResponse(impactData);
        }

        public override bool ValidateWorkFor(AbilityImpactData impactData)
        {
            if (!AnyContextTag)
            {
                var cTags = impactData.SourcedModifier.Derivation.GetContextTags();
                if (ImpactAbilityContextTags.Any(cTag => !cTags.Contains(cTag))) return false;
            }
            if (!AllowSelfImpact && impactData.SourcedModifier.Derivation.GetSource() == impactData.Target) return false;  // If self-inflicted impact is not allowed
            return impactData.Attribute == ImpactedAttribute
                   && GASHelper.ValidateImpactTypes(impactData.SourcedModifier.Derivation.GetImpactType(), ImpactType)
                   && GASHelper.ValidateImpactTargets(ImpactTarget, impactData.RealImpact, ImpactTargetExclusive)
                   && GASHelper.ValidateSignPolicy(ImpactSign, ImpactTarget, impactData.RealImpact);

        }

        protected abstract void PerformImpactResponse(AbilityImpactData impactData);

        public override AttributeScriptableObject GetTargetedAttribute()
        {
            return ImpactedAttribute;
        }
    }
    
}
