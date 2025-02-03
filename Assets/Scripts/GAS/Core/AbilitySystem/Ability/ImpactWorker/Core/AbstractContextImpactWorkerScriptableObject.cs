using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(fileName = "AEW_Context", menuName = "FESGAS/Ability/Worker/Lifesteal", order = 0)]
    public abstract class AbstractContextImpactWorkerScriptableObject : AbstractImpactWorkerScriptableObject
    {
        [Header("Contextual Impact Worker")]
        
        [Tooltip("The attribute to pull from the cache")]
        public AttributeScriptableObject PrimaryAttribute;
        public bool ApplySameFrame = true;
        
        [Space]
        
        [Tooltip("The sourced ability context tag must exist in this list")]
        public List<GameplayTagScriptableObject> ContextTags;
        [Tooltip("The impacted attribute must exist in this list")]
        public List<AttributeScriptableObject> Attributes;
        
        public override void InterpretImpact(AbilityImpactData impactData)
        {
            if (!ContextTags.Contains(impactData.SourcedModifier.Derivation.GetEffectDerivation().GetContextTag())) return;
            if (!Attributes.Contains(impactData.Attribute)) return;
            
            PerformImpactResponse(impactData);
        }

        protected abstract void PerformImpactResponse(AbilityImpactData impactData);
    }
    
}
