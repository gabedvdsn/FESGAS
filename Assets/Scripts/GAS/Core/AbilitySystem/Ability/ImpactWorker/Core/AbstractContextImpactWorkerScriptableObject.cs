using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace FESGameplayAbilitySystem
{
    public abstract class AbstractContextImpactWorkerScriptableObject : AbstractImpactWorkerScriptableObject
    {
        [FormerlySerializedAs("ContextAttribute")]
        [Header("Impact Context")]
        
        [Tooltip("The attribute must be the target attribute in the impact")]
        public AttributeScriptableObject ImpactedAttribute;
        public EImpactTypeAny ImpactType;
        public EEffectImpactTarget ImpactTarget;
        public ESignPolicy ImpactSign;
        
        [Space(5)]
        
        public bool AnyContextTag;
        [FormerlySerializedAs("AbilityContextTags")] [Tooltip("The sourced ability context tag must exist in this list")]
        public List<GameplayTagScriptableObject> ImpactAbilityContextTags;
        
        [Header("Impact Work Response")] 
        
        [Tooltip("The attribute to apply work on")]
        public AttributeScriptableObject WorkAttribute;
        public EImpactType WorkImpactType;
        public ESignPolicy WorkSignPolicy;
        
        [Space(5)]
        
        public bool WorkSameFrame = true;
        
        public override void InterpretImpact(AbilityImpactData impactData)
        {
            if (impactData.Attribute != ImpactedAttribute) return;  // If the context is not found
            if (!GASHelper.ValidateImpactTypes(impactData.SourcedModifier.Derivation.GetImpactType(), ImpactType)) return;  // If the impact type is not applicable
            if (!AnyContextTag && !ImpactAbilityContextTags.Contains(impactData.SourcedModifier.Derivation.GetEffectDerivation().GetContextTag())) return;
            
            switch (ImpactTarget)
            {
                case EEffectImpactTarget.Current:
                    if (impactData.RealImpact.CurrentValue == 0f) return;
                    if (GASHelper.DeterminePolicy(impactData.RealImpact.CurrentValue) != ImpactSign) return;
                    break;
                case EEffectImpactTarget.Base:
                    if (impactData.RealImpact.BaseValue == 0f) return;  
                    if (GASHelper.DeterminePolicy(impactData.RealImpact.BaseValue) != ImpactSign) return;
                    break;
                case EEffectImpactTarget.CurrentAndBase:
                    if (impactData.RealImpact is { CurrentValue: 0f, BaseValue: 0f }) return;
                    if (GASHelper.DeterminePolicy(impactData.RealImpact.CurrentValue) != ImpactSign) return;
                    if (GASHelper.DeterminePolicy(impactData.RealImpact.BaseValue) != ImpactSign) return;

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            PerformImpactResponse(impactData);
        }

        protected abstract void PerformImpactResponse(AbilityImpactData impactData);
    }
    
}
