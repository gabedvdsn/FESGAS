using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(menuName = "FESGAS/Effect/Gameplay Effect", fileName = "GE_")]
    public class GameplayEffectScriptableObject : ScriptableObject, IEffectBase
    {
        [Header("Gameplay Effect")]
        
        public GameplayTagScriptableObject Identifier;
        public GameplayTagScriptableObject[] GrantedTags;

        [Header("Specifications")] 
        
        public GameplayEffectImpactSpecification ImpactSpecification;
        public GameplayEffectDurationSpecification DurationSpecification;
        
        [Header("Effect Workers")]
        
        public List<AbstractEffectWorkerScriptableObject> Workers;
        
        [Header("Requirements")]
        
        public GameplayEffectRequirements SourceRequirements;
        public GameplayEffectRequirements TargetRequirements;

        [Header("Interactions")] 
        
        public GameplayTagScriptableObject[] RemoveEffectsWithTag;

        public GameplayEffectSpec Generate(IEffectDerivation derivation, GASComponentBase target)
        {
            GameplayEffectSpec spec = new GameplayEffectSpec(this, derivation, target);
            ApplyImpactSpecification(spec);

            IEffectBase effect = EffectBuilder.Prototype()
                .SetAttributeTarget(ImpactSpecification.AttributeTarget)
                .ProvideEmptyRequirements(true)
                .TryToEffect(out var e) ? e : null;
            
            return spec;
        }
        public GameplayTagScriptableObject GetIdentifier()
        {
            return Identifier;
        }
        public string GetReferenceName()
        {
            return name;
        }
        public EAffiliationPolicy GetAffiliationPolicy()
        {
            return ImpactSpecification.AffiliationPolicy;
        }
        public void ApplyImpactSpecification(GameplayEffectSpec spec)
        {
            ImpactSpecification.ApplyImpactSpecifications(spec);
        }
        
        #region Effect Base
        public AttributeScriptableObject GetAttributeTarget()
        {
            return ImpactSpecification.AttributeTarget;
        }
        public float GetMagnitude(GameplayEffectSpec spec)
        {
            return ImpactSpecification.GetMagnitude(spec);
        }
        public float GetTotalDuration(GameplayEffectSpec spec)
        {
            return DurationSpecification.GetTotalDuration(spec);
        }
        public ECalculationOperation GetImpactOperation()
        {
            return ImpactSpecification.ImpactOperation;
        }
        public EEffectImpactTarget GetTargetImpact()
        {
            return ImpactSpecification.TargetImpact;
        }
        public EImpactType GetImpactType()
        {
            return ImpactSpecification.ImpactType;
        }
        public List<AbstractEffectWorkerScriptableObject> GetEffectWorkers()
        {
            return Workers;
        }
        public bool GetReverseImpactOnRemoval()
        {
            return ImpactSpecification.ReverseImpactOnRemoval;
        }
        public EEffectReApplicationPolicy GetReApplicationPolicy()
        {
            return ImpactSpecification.ReApplicationPolicy;
        }
        public bool GetTickOnApplication()
        {
            return DurationSpecification.TickOnApplication;
        }
        public List<IEffectBase> GetContainedEffects(EApplyTickRemove policy)
        {
            return ImpactSpecification.GetContainedEffects(policy).Cast<IEffectBase>().ToList();
        }
        public EEffectDurationPolicy GetDurationPolicy()
        {
            return DurationSpecification.DurationPolicy;
        }
        public IEnumerable<GameplayTagScriptableObject> GetGrantedTags()
        {
            return GrantedTags;
        }
        public bool ValidateApplicationRequirements(GameplayEffectSpec spec)
        {
            var targetTags = spec.Target.TagCache.GetAppliedTags();
            var sourceTags = spec.Source.GetAppliedTags();
            return TargetRequirements.CheckApplicationRequirements(targetTags)
                   && !TargetRequirements.CheckRemovalRequirements(targetTags)
                   && SourceRequirements.CheckApplicationRequirements(sourceTags)
                   && !SourceRequirements.CheckRemovalRequirements(sourceTags);
        }
        public bool ValidateRemovalRequirements(GameplayEffectSpec spec)
        {
            return TargetRequirements.CheckRemovalRequirements(spec.Target.TagCache.GetAppliedTags())
                   && SourceRequirements.CheckRemovalRequirements(spec.Source.GetAppliedTags());
        }
        public bool ValidateOngoingRequirements(GameplayEffectSpec spec)
        {
            return TargetRequirements.CheckOngoingRequirements(spec.Target.TagCache.GetAppliedTags())
                   && SourceRequirements.CheckOngoingRequirements(spec.Source.GetAppliedTags());
        }
        public void ApplyDurationSpecifications(AbstractGameplayEffectShelfContainer container)
        {
            DurationSpecification.ApplyDurationSpecifications(container);
        }
        #endregion

        private void OnValidate()
        {
            if (DurationSpecification.PresetTickRatePolicy != EDefaultTickRate.None)
            {
                DurationSpecification.Ticks = Mathf.FloorToInt(DurationSpecification.Duration * GASRateNormals.GetDefaultTickRate(DurationSpecification.PresetTickRatePolicy));
            }
        }

        public override string ToString()
        {
            return $"GE-{Identifier.Name}";
        }
        
        

    }

    public enum EEffectReApplicationPolicy
    {
        Append,  // Create another instance of the effect independent of the existing one(s)
        Refresh,  // Refresh the duration of the effect
        Extend,  // Extend the duration of the effect
        Stack,  // Inject a duration-independent stack of the effect into the existing one 
        StackRefresh,  // Stack and refresh the duration of each stack
        StackExtend  // Stacks and extend the duration of each stack
    }

    /// <summary>
    /// Effect Derivations are sources which store effects (attribute impact derivations)
    /// </summary>
    public interface IEffectDerivation
    {
        public ISource GetOwner();
        public List<ITag> GetContextTags();
        public ITag GetAssetTag();
        public int GetLevel();
        public void SetLevel(int level);
        public float GetRelativeLevel();
        public string GetName();
        public ITag GetAffiliation();

        public static SourceEffectDerivation GenerateSourceDerivation(ISource source)
        {
            return new SourceEffectDerivation(source);
        }
    }

    public class SourceEffectDerivation : IEffectDerivation
    {
        private ISource Owner;

        public SourceEffectDerivation(ISource owner)
        {
            Owner = owner;
        }

        public ISource GetOwner()
        {
            return Owner;
        }
        public List<ITag> GetContextTags()
        {
            return Owner.GetContextTags();
        }
        public ITag GetAssetTag()
        {
            return Owner.GetAssetTag();
        }
        public int GetLevel()
        {
            return Owner.GetLevel();
        }
        public void SetLevel(int level)
        {
            Owner.SetLevel(level);
        }
        public float GetRelativeLevel()
        {
            return (Owner.GetLevel() - 1) / (float)(Owner.GetMaxLevel() - 1);
        }
        public string GetName()
        {
            return Owner.GetName();
        }
        public ITag GetAffiliation()
        {
            return Owner.GetAffiliation();
        }
    }

}
