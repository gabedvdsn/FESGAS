using System;
using System.Collections.Generic;

namespace FESGameplayAbilitySystem
{
    public interface IEffectBase
    {
        public AttributeScriptableObject GetAttributeTarget();
        public float GetMagnitude(GameplayEffectSpec spec);
        public float GetTotalDuration(GameplayEffectSpec spec);
        public ECalculationOperation GetImpactOperation();
        public EEffectImpactTarget GetTargetImpact();
        public EImpactType GetImpactType();
        public List<AbstractEffectWorkerScriptableObject> GetEffectWorkers();
        public bool GetReverseImpactOnRemoval();
        public EEffectReApplicationPolicy GetReApplicationPolicy();
        public bool GetTickOnApplication();
        public List<IEffectBase> GetContainedEffects(EApplyDuringRemove policy);
        public EEffectDurationPolicy GetDurationPolicy();
        public IEnumerable<GameplayTagScriptableObject> GetGrantedTags();
        public bool ValidateApplicationRequirements(GameplayEffectSpec spec);
        public bool ValidateRemovalRequirements(GameplayEffectSpec spec);
        public bool ValidateOngoingRequirements(GameplayEffectSpec spec);
        public void ApplyDurationSpecifications(AbstractGameplayEffectShelfContainer container);
        public GameplayEffectSpec Generate(IEffectDerivation derivation, GASComponentBase system);
        public GameplayTagScriptableObject GetIdentifier();
        public string GetReferenceName();
        public EAffiliationPolicy GetAffiliationPolicy();
    }

    public static class EffectBuilder
    {
        public static EffectPrototype Prototype()
        {
            return new EffectPrototype();
        }
        
        public class EffectPrototype
        {
            private CustomEffect Effect = new();
            private bool createEmptyRequirements;
            
            public bool TryToEffect(out CustomEffect effect)
            {
                effect = null;
                
                if (!Effect.AttributeTarget) return false;

                if (Effect.SourceRequirements is null || Effect.TargetRequirements is null)
                {
                    if (!createEmptyRequirements) return false;
                    if (Effect.SourceRequirements is null) Effect.SourceRequirements = IEffectRequirements.GenerateEmptyRequirements();
                    if (Effect.TargetRequirements is null) Effect.TargetRequirements = IEffectRequirements.GenerateEmptyRequirements();
                }

                effect = Effect;
                return true;
            }

            public CustomEffect ToEffectUnvalidated()
            {
                return Effect;
            }

            public EffectPrototype SetAttributeTarget(AttributeScriptableObject arg)
            {
                Effect.AttributeTarget = arg;
                return this;
            }
            
            public EffectPrototype SetMagnitude(float arg)
            {
                Effect.ImpactValue = arg;
                return this;
            }

            public EffectPrototype SetImpactSpecification(GameplayEffectImpactSpecification arg)
            {
                Effect.ImpactSpecification = arg;
                return this;
            }

            public EffectPrototype SetImpactModifier(IMagnitudeModifier arg, EMagnitudeOperation operation)
            {
                Effect.ImpactModifier = arg;
                Effect.ImpactMagnitudeOperation = operation;
                return this;
            }

            public EffectPrototype SetUseImpactModifier(bool flag)
            {
                Effect.UseImpactModifier = flag;
                return this;
            }
            
            public EffectPrototype SetTotalDuration(float arg)
            {
                Effect.DurationValue = arg;
                return this;
            }

            public EffectPrototype SetDurationSpecification(GameplayEffectDurationSpecification arg)
            {
                Effect.DurationSpecification = arg;
                return this;
            }
            
            public EffectPrototype SetDurationModifier(IMagnitudeModifier arg, EMagnitudeOperation operation)
            {
                Effect.DurationModifier = arg;
                Effect.DurationMagnitudeOperation = operation;
                return this;
            }
            
            public EffectPrototype SetUseDurationModifier(bool flag)
            {
                Effect.UseDurationModifier = flag;
                return this;
            }
            
            public EffectPrototype SetImpactOperation(ECalculationOperation arg)
            {
                Effect.ImpactOperation = arg;
                return this;
            }
            
            public EffectPrototype SetTargetImpact(EEffectImpactTarget arg)
            {
                Effect.TargetImpact = arg;
                return this;
            }
            
            public EffectPrototype SetImpactType(EImpactType arg)
            {
                Effect.ImpactType = arg;
                return this;
            }
            
            public EffectPrototype SetEffectWorkers(List<AbstractEffectWorkerScriptableObject> arg)
            {
                Effect.EffectWorkers = arg;
                return this;
            }
            
            public EffectPrototype SetReverseImpactOnRemoval(bool arg)
            {
                Effect.ReverseOnRemoval = arg;
                return this;
            }
            
            public EffectPrototype SetReApplicationPolicy(EEffectReApplicationPolicy arg)
            {
                Effect.ReApplicationPolicy = arg;
                return this;
            }
            
            public EffectPrototype SetTickOnApplication(bool arg)
            {
                Effect.TickOnApplication = arg;
                return this;
            }
            
            public EffectPrototype SetContainedEffects(List<IEffectBase> arg)
            {
                Effect.ContainedEffects = arg;
                return this;
            }
            
            public EffectPrototype SetDurationPolicy(EEffectDurationPolicy arg)
            {
                Effect.DurationPolicy = arg;
                return this;
            }
            
            public EffectPrototype SetGrantedTags(IEnumerable<GameplayTagScriptableObject> arg)
            {
                Effect.GrantedTags = arg;
                return this;
            }

            public EffectPrototype ProvideEmptyRequirements(bool flag)
            {
                createEmptyRequirements = flag;
                return this;
            }
            
            public EffectPrototype SetSourceRequirements(IEffectRequirements arg)
            {
                Effect.SourceRequirements = arg;
                return this;
            }
            
            public EffectPrototype SetTargetRequirements(IEffectRequirements arg)
            {
                Effect.TargetRequirements = arg;
                return this;
            }
            
            public EffectPrototype SetIdentifier(GameplayTagScriptableObject arg)
            {
                Effect.Identifier = arg;
                return this;
            }

            public EffectPrototype SetReferenceName(string arg)
            {
                Effect.ReferenceName = arg;
                return this;
            }

            public EffectPrototype SetAffiliationPolicy(EAffiliationPolicy arg)
            {
                Effect.AffiliationPolicy = arg;
                return this;
            }

        }
    }

    public class CustomEffect : IEffectBase
    {
        public AttributeScriptableObject AttributeTarget;
        public float ImpactValue;
        
        public GameplayEffectImpactSpecification ImpactSpecification;
        public IMagnitudeModifier ImpactModifier;
        public EMagnitudeOperation ImpactMagnitudeOperation;
        public bool UseImpactModifier;
        
        public GameplayEffectDurationSpecification DurationSpecification;
        public IMagnitudeModifier DurationModifier;
        public EMagnitudeOperation DurationMagnitudeOperation;
        public bool UseDurationModifier;
        public float DurationValue;

        public ECalculationOperation ImpactOperation;
        public EEffectImpactTarget TargetImpact;
        public EImpactType ImpactType;

        public List<AbstractEffectWorkerScriptableObject> EffectWorkers;
        public bool ReverseOnRemoval;
        public EEffectReApplicationPolicy ReApplicationPolicy;

        public bool TickOnApplication;
        public List<IEffectBase> ContainedEffects;
        public EEffectDurationPolicy DurationPolicy;

        public IEnumerable<GameplayTagScriptableObject> GrantedTags;
        public IEffectRequirements TargetRequirements;
        public IEffectRequirements SourceRequirements;

        public GameplayTagScriptableObject Identifier;
        public string ReferenceName;

        public EAffiliationPolicy AffiliationPolicy;
        
        public AttributeScriptableObject GetAttributeTarget() => AttributeTarget;

        public float GetMagnitude(GameplayEffectSpec spec)
        {
            if (!UseImpactModifier || ImpactModifier is null) return ImpactSpecification?.GetMagnitude(spec) ?? ImpactValue;
            
            float calculatedMagnitude = ImpactModifier.Evaluate(spec);

            return ImpactMagnitudeOperation switch
            {
                EMagnitudeOperation.Add => ImpactValue + calculatedMagnitude,
                EMagnitudeOperation.Multiply => ImpactValue * calculatedMagnitude,
                EMagnitudeOperation.UseMagnitude => ImpactValue,
                EMagnitudeOperation.UseCalculation => calculatedMagnitude,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        public float GetTotalDuration(GameplayEffectSpec spec)
        {
            if (!UseDurationModifier || DurationModifier is null) return DurationSpecification?.GetTotalDuration(spec) ?? DurationValue;
            
            float calculatedMagnitude = DurationModifier.Evaluate(spec);

            return DurationMagnitudeOperation switch
            {
                EMagnitudeOperation.Add => DurationValue + calculatedMagnitude,
                EMagnitudeOperation.Multiply => DurationValue * calculatedMagnitude,
                EMagnitudeOperation.UseMagnitude => DurationValue,
                EMagnitudeOperation.UseCalculation => calculatedMagnitude,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        public ECalculationOperation GetImpactOperation() => ImpactOperation;
        public EEffectImpactTarget GetTargetImpact() => TargetImpact;
        public EImpactType GetImpactType() => ImpactType;
        public List<AbstractEffectWorkerScriptableObject> GetEffectWorkers() => EffectWorkers;
        public bool GetReverseImpactOnRemoval() => ReverseOnRemoval;
        public EEffectReApplicationPolicy GetReApplicationPolicy() => ReApplicationPolicy;
        public bool GetTickOnApplication() => TickOnApplication;
        public List<IEffectBase> GetContainedEffects(EApplyDuringRemove policy) => ContainedEffects;
        public EEffectDurationPolicy GetDurationPolicy() => DurationPolicy;
        public IEnumerable<GameplayTagScriptableObject> GetGrantedTags() => GrantedTags;
        public bool ValidateApplicationRequirements(GameplayEffectSpec spec)
        {
            if (SourceRequirements is null || TargetRequirements is null) return false;
            
            var targetTags = spec.Target.TagCache.GetAppliedTags();
            var sourceTags = spec.Source.TagCache.GetAppliedTags();
            return TargetRequirements.CheckApplicationRequirements(targetTags)
                   && !TargetRequirements.CheckRemovalRequirements(targetTags)
                   && SourceRequirements.CheckApplicationRequirements(sourceTags)
                   && !SourceRequirements.CheckRemovalRequirements(sourceTags);
        }
        public bool ValidateRemovalRequirements(GameplayEffectSpec spec)
        {
            if (SourceRequirements is null || TargetRequirements is null) return true;
            
            return TargetRequirements.CheckRemovalRequirements(spec.Target.TagCache.GetAppliedTags())
                   && SourceRequirements.CheckRemovalRequirements(spec.Source.TagCache.GetAppliedTags());
        }
        public bool ValidateOngoingRequirements(GameplayEffectSpec spec)
        {
            if (SourceRequirements is null || TargetRequirements is null) return false;
            
            return TargetRequirements.CheckOngoingRequirements(spec.Target.TagCache.GetAppliedTags())
                   && SourceRequirements.CheckOngoingRequirements(spec.Source.TagCache.GetAppliedTags());
        }
        public void ApplyDurationSpecifications(AbstractGameplayEffectShelfContainer container)
        {
            DurationSpecification?.ApplyDurationSpecifications(container);
        }
        public GameplayEffectSpec Generate(IEffectDerivation derivation, GASComponentBase target)
        {
            GameplayEffectSpec spec = new GameplayEffectSpec(this, derivation, target);
            if (ImpactSpecification is not null) ImpactSpecification.ApplyImpactSpecifications(spec);

            return spec;
        }
        public GameplayTagScriptableObject GetIdentifier()
        {
            return Identifier;
        }
        public string GetReferenceName()
        {
            return ReferenceName;
        }
        public EAffiliationPolicy GetAffiliationPolicy()
        {
            return AffiliationPolicy;
        }
    }
}
