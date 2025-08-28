using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace FESGameplayAbilitySystem
{
    public class GameplayEffect : IEffectBase
    {
        [Header("Gameplay Effect")] 
        
        public GameplayEffectDefinition Definition;

        public GameplayEffectTags Tags;

        [Header("Specifications")] 
        
        public GameplayEffectImpactSpecification ImpactSpecification;
        public GameplayEffectDurationSpecification DurationSpecification;
        
        [Header("Effect Workers")]
        
        public List<AbstractEffectWorker> Workers;
        
        [Header("Requirements")]
        
        public TagRequirements SourceRequirements;
        public TagRequirements TargetRequirements;

        public GameplayEffectSpec Generate(IEffectOrigin origin, GASComponent target)
        {
            GameplayEffectSpec spec = new GameplayEffectSpec(this, origin, target);
            ApplyImpactSpecification(spec);

            IEffectBase effect = EffectBuilder.Prototype()
                .SetAttributeTarget(ImpactSpecification.AttributeTarget)
                .ProvideEmptyRequirements(true)
                .TryToEffect(out var e) ? e : null;
            
            return spec;
        }
        public Tag GetAssetTag()
        {
            return Tags.AssetTag;
        }
        public string GetReferenceName()
        {
            return Definition.Name;
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
        public Attribute GetAttributeTarget()
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
        public List<AbstractEffectWorker> GetEffectWorkers()
        {
            return Workers;
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
            return $"GE-{Definition.Name}";
        }
    }

    public class GameplayEffectDefinition
    {
        public string Name;
        public string Description;
        public bool Visible = true;
        public bool UseDerivationIcon = true;
        public Sprite Icon;
    }

    public interface IHasReadableDefinition
    {
        public string GetName();
        public string GetDescription();
        public Sprite GetPrimaryIcon();
    }

    public class GameplayEffectTags
    {
        public Tag AssetTag;
        public Tag[] ContextTags;
        public Tag[] GrantedTags;
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
    /// Sources of Gameplay Effects
    /// </summary>
    public interface IEffectOrigin
    {
        public ISource GetOwner();
        public Tag[] GetContextTags();
        public Tag GetAssetTag();
        public int GetLevel();
        public void SetLevel(int level);
        public float GetRelativeLevel();
        public string GetName();
        public Tag GetAffiliation();

        public static SourceEffectOrigin GenerateSourceDerivation(ISource source)
        {
            return new SourceEffectOrigin(source);
        }
    }

    public class SourceEffectOrigin : IEffectOrigin
    {
        private ISource Owner;

        public SourceEffectOrigin(ISource owner)
        {
            Owner = owner;
        }

        public ISource GetOwner()
        {
            return Owner;
        }
        public Tag[] GetContextTags()
        {
            return Owner.GetContextTags();
        }
        public Tag GetAssetTag()
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
        public Tag GetAffiliation()
        {
            return Owner.GetAffiliation();
        }
    }

}
