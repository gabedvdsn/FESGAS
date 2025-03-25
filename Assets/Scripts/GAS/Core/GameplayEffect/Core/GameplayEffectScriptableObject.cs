using System.Collections;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.Serialization;

namespace FESGameplayAbilitySystem
{
    [CreateAssetMenu(menuName = "FESGAS/Gameplay Effect", fileName = "New Gameplay Effect")]
    public class GameplayEffectScriptableObject : AbstractGameplayEffectScriptableObject
    {
        [Header("Gameplay Effect")]
        
        public GameplayTagScriptableObject Identifier;
        public GameplayTagScriptableObject[] GrantedTags;

        [Header("Specifications")] 
        
        public GameplayEffectImpactSpecification ImpactSpecification;
        public GameplayEffectDurationSpecification DurationSpecification;
        
        [Header("Requirements")]
        
        public GameplayEffectRequirements SourceRequirements;
        public GameplayEffectRequirements TargetRequirements;

        [Header("Interactions")] 
        
        public GameplayTagScriptableObject[] RemoveEffectsWithTag;

        public override GameplayEffectSpec Generate(IEffectDerivation derivation, GASComponent target)
        {
            GameplayEffectSpec spec = new GameplayEffectSpec(this, derivation, target);
            ApplyImpactSpecification(spec);

            return spec;
        }
        public override void ApplyImpactSpecification(GameplayEffectSpec spec)
        {
            ImpactSpecification.ApplyImpactSpecifications(spec);
        }

        private void OnValidate()
        {
            if (DurationSpecification.UseDefaultTickRate && DurationSpecification.Duration > 0f)
            {
                DurationSpecification.TickOnApplication = false;
                DurationSpecification.Ticks = Mathf.FloorToInt(DurationSpecification.Duration * (1 / GASRateNormals.DEFAULT_TICK_PERIOD));
            }
        }

        public override string ToString()
        {
            return $"GE-{Identifier.Name}";
        }

    }

    public enum GameplayEffectApplicationPolicy
    {
        Append,  // Create another instance of the effect independent of the existing one(s)
        Refresh,  // Refresh the duration of the effect
        Extend,  // Extend the duration of the effect
        Stack,  // Inject a duration-independent stack of the effect into the existing one 
        StackRefresh,  // Stack and refresh the duration of each stack
        StackExtend  // Stacks and extend the duration of each stack
    }

    public interface IEffectDerivation
    {
        public GASComponent GetOwner();
        public GameplayTagScriptableObject GetContextTag();
        public int GetLevel();
        public void SetLevel(int level);
        public float GetRelativeLevel();
        public string GetName();

        public static SourceEffectDerivation GenerateSourceDerivation(GASComponent source)
        {
            return new SourceEffectDerivation(source);
        }
    }

    public class SourceEffectDerivation : IEffectDerivation
    {
        private GASComponent Owner;

        public SourceEffectDerivation(GASComponent owner)
        {
            Owner = owner;
        }

        public GASComponent GetOwner()
        {
            return Owner;
        }
        public GameplayTagScriptableObject GetContextTag()
        {
            return Owner.Data.NameTag;
        }
        public int GetLevel()
        {
            return Owner.Data.Level;
        }
        public void SetLevel(int level)
        {
            Owner.Data.Level = level;
        }
        public float GetRelativeLevel()
        {
            return (Owner.Data.Level - 1) / (float)(Owner.Data.MaxLevel - 1);
        }
        public string GetName()
        {
            return Owner.Data.DistinctName;
        }
    }

}
